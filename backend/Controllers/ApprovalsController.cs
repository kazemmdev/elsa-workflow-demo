using Elsa.Workflows;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Filters;
using Elsa.Workflows.Runtime.Stimuli;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ElsaWorkflow.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(
    IBookmarkStore bookmarkStore,
    ITaskReporter taskReporter,
    IPayloadSerializer payloadSerializer) : ControllerBase
{
    [HttpGet("{instanceId}/tasks")]
    public async Task<IActionResult> GetPendingTasks(
        string instanceId,
        CancellationToken cancellationToken)
    {
        var filter = new BookmarkFilter { WorkflowInstanceId = instanceId };
        var bookmarks = await bookmarkStore.FindManyAsync(filter, cancellationToken);

        var tasks = bookmarks
            .Where(b => b.Name == "Elsa.RunTask")
            .Select(b =>
            {
                RunTaskStimulus? p = null;
                try { p = DeserializeStimulus(b.Payload); }
                catch { /* ignore */ }

                return new
                {
                    taskName = p?.TaskName,
                    taskId = p?.TaskId,
                    activityInstanceId = b.ActivityInstanceId
                };
            })
            .ToList();

        return Ok(tasks);
    }

    [HttpPost("{instanceId}/manager")]
    public async Task<IActionResult> SubmitManagerApproval(
        string instanceId,
        [FromBody] ApprovalDecisionDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Decision is not ("approved" or "rejected"))
            return BadRequest(new { error = "Decision must be 'approved' or 'rejected'" });

        var payload = await FindTaskPayloadAsync(instanceId, "ManagerApproval", cancellationToken);

        if (payload is null)
            return NotFound(new { error = "No pending ManagerApproval task found for this workflow instance" });

        await taskReporter.ReportCompletionAsync(payload.TaskId, dto.Decision, cancellationToken);

        return Ok(new { message = $"Manager decision '{dto.Decision}' submitted successfully" });
    }

    [HttpPost("{instanceId}/finance")]
    public async Task<IActionResult> SubmitFinanceApproval(
        string instanceId,
        [FromBody] ApprovalDecisionDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Decision is not ("approved" or "rejected"))
            return BadRequest(new { error = "Decision must be 'approved' or 'rejected'" });

        var payload = await FindTaskPayloadAsync(instanceId, "FinanceApproval", cancellationToken);

        if (payload is null)
            return NotFound(new { error = "No pending FinanceApproval task found for this workflow instance" });

        await taskReporter.ReportCompletionAsync(payload.TaskId, dto.Decision, cancellationToken);

        return Ok(new { message = $"Finance decision '{dto.Decision}' submitted successfully" });
    }

    private async Task<RunTaskStimulus?> FindTaskPayloadAsync(
        string instanceId,
        string taskName,
        CancellationToken cancellationToken)
    {
        var filter = new BookmarkFilter { WorkflowInstanceId = instanceId };
        var bookmarks = await bookmarkStore.FindManyAsync(filter, cancellationToken);

        foreach (var bookmark in bookmarks.Where(b => b.Name == "Elsa.RunTask"))
        {
            try
            {
                var payload = DeserializeStimulus(bookmark.Payload);
                if (payload?.TaskName == taskName)
                    return payload;
            }
            catch { /* skip unreadable bookmarks */ }
        }

        return null;
    }

    private RunTaskStimulus? DeserializeStimulus(object? raw)
    {
        return raw switch
        {
            RunTaskStimulus s  => s,
            JsonElement el     => payloadSerializer.Deserialize<RunTaskStimulus>(el),
            string json        => payloadSerializer.Deserialize<RunTaskStimulus>(json),
            _                  => payloadSerializer.Deserialize<RunTaskStimulus>(
                                      JsonSerializer.SerializeToElement(raw))
        };
    }
}

public record ApprovalDecisionDto(string Decision);
