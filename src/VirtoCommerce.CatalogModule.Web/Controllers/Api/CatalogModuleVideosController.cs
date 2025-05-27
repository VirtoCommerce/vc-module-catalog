using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Options;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/videos")]
    public class CatalogModuleVideosController(
        IVideoSearchService videoSearchService,
        IVideoService videoService,
        IOptions<VideoOptions> videoOptions
        ) : Controller
    {
        /// <summary>
        /// Get video options from configuration
        /// </summary>
        [HttpGet]
        [Route("options")]
        [ProducesResponseType(typeof(VideoOptions), StatusCodes.Status200OK)]
        [Authorize(ModuleConstants.Security.Permissions.Access)]
        public ActionResult<VideoOptions> GetOptions()
        {
            return Ok(videoOptions.Value);
        }

        /// <summary>
        /// Create video
        /// </summary>
        [HttpPost]
        [Route("create")]
        [ProducesResponseType(typeof(Video), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Video>> CreateVideo([FromBody] VideoCreateRequest createRequest)
        {
            if (!createRequest.SortOrder.HasValue)
            {
                var searchCriteria = new VideoSearchCriteria
                {
                    OwnerIds = [createRequest.OwnerId],
                    OwnerType = createRequest.OwnerType,
                    Sort = "SortOrder:desc",
                    Skip = 0,
                    Take = 1,
                };

                var searchResult = await videoSearchService.SearchNoCloneAsync(searchCriteria);

                createRequest.SortOrder = searchResult.Results.Count != 0 ? searchResult.Results[0].SortOrder + 1 : 1;
            }

            try
            {
                var video = await videoService.CreateVideo(createRequest);

                return Ok(video);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        /// <summary>
        /// Search videos
        /// </summary>
        [HttpPost]
        [Route("search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<VideoSearchResult>> SearchVideos([FromBody] VideoSearchCriteria criteria)
        {
            var result = await videoSearchService.SearchNoCloneAsync(criteria);

            return result;
        }

        /// <summary>
        ///  Create new or update existing videos
        /// </summary>
        /// <param name="videos">Video models</param>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(Video[]), StatusCodes.Status200OK)]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Video[]>> Update([FromBody] Video[] videos)
        {
            await videoService.SaveChangesAsync(videos);

            return Ok(videos);
        }

        /// <summary>
        /// Delete videos by ids
        /// </summary>
        /// <param name="ids">Video ids</param>
        [HttpDelete]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public async Task<ActionResult> Delete([FromQuery] string[] ids)
        {
            await videoService.DeleteAsync(ids);

            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified Video by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchVideo(string id, [FromBody] JsonPatchDocument<Video> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var video = await videoService.GetByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(video, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await videoService.SaveChangesAsync([video]);

            return NoContent();
        }
    }
}
