using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GenericApiWrapper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        public ActionResult<ApiResponse<List<string>>> Get()
        {
            var data = new List<string> { "Item1", "Item2", "Item3" };
            var response = ApiResponse<List<string>>.SuccessResponse(data, "Items retrieved successfully");
            return Ok(response);
        }

        [HttpGet("paginated")]
        public ActionResult<ApiResponse<List<string>>> GetPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 2)
        {
            var allData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            
            var totalItems = allData.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var paginatedData = allData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = ApiResponse<List<string>>.PaginatedResponse(
                paginatedData,
                pageNumber,
                pageSize,
                totalItems,
                totalPages,
                "Items retrieved successfully"
            );
            
            return Ok(response);
        }

        [HttpGet("error")]
        public ActionResult<ApiResponse<string>> GetError()
        {
            var response = ApiResponse<string>.ErrorResponse(
                "RESOURCE_NOT_FOUND",
                "The requested resource was not found",
                404
            );
            return NotFound(response);
        }
    }
}
