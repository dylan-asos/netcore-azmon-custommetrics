namespace Asos.AzMon.CustomMetrics.Model
{
    internal class ServiceBusEntity
    {
        const decimal MegaBytesInBytes = 1048576;

        public ServiceBusEntity()
        {            
        }

        public ServiceBusEntity(string namespaceResourceId, long currentSizeInBytes, long maxSizeInMb, string name)
        {
            NamespaceResourceId = namespaceResourceId;
            CurrentSizeInBytes = currentSizeInBytes;
            MaxSizeInMb = maxSizeInMb;
            Name = name;
        }

        public string NamespaceResourceId {get; set; }
        public decimal MaxSizeInMb {get; set; }
        public decimal CurrentSizeInBytes {get; set; }
        public string Name {get; set; }

        public decimal PercentageOfCapacityUsed => CalculateCapacityPercentageUsed();

        public decimal CalculateCapacityPercentageUsed()
        {            
            var topicMaxSizeInBytes = MaxSizeInMb * MegaBytesInBytes;

            var capcityUsedPercent = CurrentSizeInBytes > topicMaxSizeInBytes
                ? 100
                : decimal.Round(CurrentSizeInBytes / topicMaxSizeInBytes * 100, 5);
          
            return capcityUsedPercent;
        }
    }
}