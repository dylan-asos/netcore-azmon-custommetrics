namespace Asos.AzMon.CustomMetrics.Model
{
    internal class ServiceBusEntity
    {
        public string NamespaceResourceId {get; set; }
        public decimal MaxSizeInMb {get; set; }
        public decimal CurrentSizeInBytes {get; set; }
        public string Name {get; set; }
    }
}