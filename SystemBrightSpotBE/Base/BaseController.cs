using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace SystemBrightSpotBE.Base
{
    public class BaseController : ControllerBase
    {
        protected JsonResult JJsonResponse(int StatusCode, string? Message = null, object? Data = null, object? ErrorDetails = null, string? ErrorMessage = null)
        {
            BaseResponse response = new BaseResponse(Message, Data, ErrorDetails, ErrorMessage);
            return new JsonResult(response)
            {
                StatusCode = StatusCode
            };
        }

        protected BaseResponse Success()
        {
            BaseResponse result = new BaseResponse();
            return result;
        }
        protected BaseResponse Success(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            BaseResponse result = new BaseResponse();
            result.Data = data;
            return result;
        }

        protected BaseResponse Error(string message)
        {
            BaseResponse result = new BaseResponse();
            result.Message = message;
            return result;
        }
        protected BaseResponse Error(ModelStateDictionary modelState)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors).ToList();
            var list = new List<string>();
            foreach (var item in errors)
            {
                if (string.IsNullOrWhiteSpace(item.ErrorMessage))
                {
                    if (item.Exception is not null)
                    {
                        list.Add(item.Exception.Message);
                    }
                }
                else
                {
                    list.Add(item.ErrorMessage);
                }
            }
            string messages = string.Join("; ", list);
            return Error(messages);
        }
        protected BaseResponse Error(string message, List<BaseError>? errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            BaseResponse result = new BaseResponse();
            result.Message = message;
            result.ErrorDetails = errors;
            return result;
        }
        protected BaseResponse Error(string message, BaseError? error = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            List<BaseError> errors = new List<BaseError>();
            if (error is not null)
            {
                errors.Add(error);
            }
            BaseResponse result = new BaseResponse();
            result.Message = message;
            result.ErrorDetails = errors;
            return result;
        }
    }
}
