using KonbiBrain.Common.Messages;

namespace KonbiBrain.Messages
{
    public class TemperatureCommands
    {
        public class CurrentTemprature : BaseCommand
        {
            public CurrentTemprature()
            {
                Command = CommunicationCommands.Temperature_CurrentTemperature;
            }
            public double CurrentTemperature { get; set; }
        }

        public class SetTemperature : BaseCommand
        {
            public SetTemperature()
            {
                Command = CommunicationCommands.Temperature_SetTemperature;
            }

            public int Temperature { get; set; }
        }
        public class GetTemperature : BaseCommand
        {
            public GetTemperature()
            {
                Command = CommunicationCommands.Temperature_GetTemperature;
            }
            
        }
    }
}