using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        public PlatformsController(IPlatformRepo repository, IMapper mapper,
        
        ICommandDataClient commandDataClient)
        {
            _commandDataClient = commandDataClient;
            _mapper = mapper;
            _repository = repository;
        }

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {

        var platformItem = _repository.GetAllPlatforms();
        return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
    }

    [HttpGet("{id}", Name = "GetPlatformById")]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var platformItem = _repository.GetPlatformById(id);
        if (platformItem != null)
        {
            return Ok(_mapper.Map<PlatformReadDto>(platformItem));
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreateNewPlatform(PlatformCreateDto platformToCreate)
    {
        var platformToAdd = _mapper.Map<Platform>(platformToCreate);
        _repository.CreatePlatform(platformToAdd);
        _repository.SaveChanges();

        var returnPlatform = _mapper.Map<PlatformReadDto>(platformToAdd);
        try 
        {
            await _commandDataClient.SendPlatformToCommand(returnPlatform);
        } 
        catch(Exception ex)
        {
            Console.WriteLine($"--> Could not send syncrhonously: {ex.Message}");
        }
        
        return CreatedAtRoute(nameof(GetPlatformById), new { Id = returnPlatform.Id }, returnPlatform);
    }
}


}