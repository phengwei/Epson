using System.Collections.Generic;
using Epson.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    public class BaseApiController : Controller
    {
        protected IActionResult Ok(string message)
        {
            var model = new BaseResponseModel();
            model.Message = message;
            return Ok(model);
        }

        protected IActionResult Created(string message)
        {
            var model = new BaseResponseModel();
            model.Message = message;
            return StatusCode(StatusCodes.Status201Created, model);
        }

        protected IActionResult BadRequest(string error)
        {
            var model = new BaseResponseModel();
            model.ErrorList.Add(error);
            return BadRequest(model);
        }

        protected IActionResult BadRequest(List<string> errors)
        {
            var model = new BaseResponseModel();
            model.ErrorList.AddRange(errors);
            return BadRequest(model);
        }

        protected IActionResult Unauthorized(string error)
        {
            var model = new BaseResponseModel();
            model.ErrorList.Add(error);
            return Unauthorized(model);
        }

        protected IActionResult NotFound(string error)
        {
            var model = new BaseResponseModel();
            model.ErrorList.Add(error);
            return NotFound(model);
        }

        protected IActionResult MethodNotAllowed()
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        protected IActionResult LengthRequired()
        {
            return StatusCode(StatusCodes.Status411LengthRequired);
        }

        protected IActionResult InternalServerError(string error)
        {
            var model = new BaseResponseModel();
            model.ErrorList.Add(error);
            return StatusCode(StatusCodes.Status500InternalServerError, model);
        }
    }
}
