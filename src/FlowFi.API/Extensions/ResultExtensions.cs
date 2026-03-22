using FlowFi.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller) =>
        result.IsSuccess
            ? controller.Ok(new { success = true, data = result.Value })
            : result.ErrorCode switch
            {
                "NOT_FOUND"      => controller.NotFound(ToError(result)),
                "UNAUTHORIZED"   => controller.Unauthorized(ToError(result)),
                "TOKEN_EXPIRED"  => controller.Unauthorized(ToError(result)),
                "FORBIDDEN"      => controller.StatusCode(403, ToError(result)),
                "PLAN_REQUIRED"  => controller.StatusCode(403, ToError(result)),
                "CONFLICT"       => controller.Conflict(ToError(result)),
                "VALIDATION_ERROR" => controller.BadRequest(ToError(result)),
                _                => controller.StatusCode(500, ToError(result)),
            };

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result, ControllerBase controller, string? location = null) =>
        result.IsSuccess
            ? controller.Created(location ?? string.Empty, new { success = true, data = result.Value })
            : result.ToActionResult(controller);

    public static IActionResult ToActionResult(this Result result, ControllerBase controller) =>
        result.IsSuccess
            ? controller.NoContent()
            : result.ErrorCode switch
            {
                "NOT_FOUND"      => controller.NotFound(ToError(result)),
                "UNAUTHORIZED"   => controller.Unauthorized(ToError(result)),
                "FORBIDDEN"      => controller.StatusCode(403, ToError(result)),
                _                => controller.StatusCode(500, ToError(result)),
            };

    private static object ToError<T>(Result<T> r) =>
        new { success = false, error = new { code = r.ErrorCode, message = r.Error } };

    private static object ToError(Result r) =>
        new { success = false, error = new { code = r.ErrorCode, message = r.Error } };
}
