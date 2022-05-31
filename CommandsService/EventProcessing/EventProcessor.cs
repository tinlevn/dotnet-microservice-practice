using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            this._mapper = mapper;
            this._scopeFactory = scopeFactory;

        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch(eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:

                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage){
            Console.WriteLine("--> Determining Event");
            
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            switch(eventType.Event)
            {
                case "Platform_published":
                    Console.WriteLine("Platform Published Event detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("Platform Published Event detected");
                    return EventType.Undetermined;
            }
        }

        private void addPlatform(string platformPublishedMessage) 
        {
            using (var scope=_scopeFactory.CreateScope()){
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                
                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try 
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if(!repo.ExternalPlatformExist(plat.ExternalId))
                    {
                        repo.CreatPlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine("---> Platform added!");
                    } else {
                        Console.WriteLine("---> Platform already exists");
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"---> Could not add Platform to DB {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}