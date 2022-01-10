using Symnity.Model.Metadatas;

namespace Symnity.Http.Model
{
    public struct MetadataSearchCriteria
    {
        public readonly MetadataType MetadataType;
        public readonly string SourceAddress;
        public readonly string ScopedMetadataKey;
        public readonly string Id;
        public MetadataSearchCriteria(
            MetadataType metadataType,
            string sourceAddress,
            string scopedMetadataKey,
            string id
        )
        {
            MetadataType = metadataType;
            SourceAddress = sourceAddress;
            ScopedMetadataKey = scopedMetadataKey;
            Id = id;
        }
    }
}