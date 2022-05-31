using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;
        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            this._mapper = mapper;
            this._repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"=> Hit GetCommandsForPlatform: {platformId}");

            if (!_repository.PlatformExist(platformId))
            {
                return NotFound();
            } 

            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
            
        }

        [HttpGet("{commandId}", Name = "GetSingleCommand")]
        public ActionResult<CommandReadDto> GetSingleCommand(int platformId, int commandId)
        {
            Console.WriteLine($"=> Hit GetSingleCommand: {platformId} / {commandId}");
            if (!_repository.PlatformExist(platformId))
            {
                return NotFound();
            } 
            var command = _repository.GetCommand(platformId, commandId);
            
            if (command == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
        {
            Console.WriteLine($"=> Hit CreateCommandForPlatform: {platformId}");
            if (!_repository.PlatformExist(platformId))
            {
                return NotFound();
            } 

            var command = _mapper.Map<Command>(commandDto);

            _repository.CreateCommand(platformId, command);            
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(GetSingleCommand), 
                new {platformId = platformId, commandId= commandReadDto.Id}, commandReadDto);
        }
    }
}