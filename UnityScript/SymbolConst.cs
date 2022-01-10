using Symnity.Model.Accounts;
using Symnity.Model.Network;

namespace Symnity.UnityScript
{
    public static class SymbolConst
    {
        public const NetworkType Mode = NetworkType.TEST_NET;
        public const string WsNode = "ws://test.hideyoshi-node.net:3000/ws";
        public const string Node = "https://test.hideyoshi-node.net:3001";
        public const string MosaicId = "3A8416DB2D53B6C8";
        public const string CoinMosaicId = "65DBB4CC472A5734";
        public const string AdminPrivateKey = "C4A87F3B600EBE31D6DBA29FD3E68396F34D362B0C2252825B506006B4BBC45C";
        public static Account AdminAccount = Account.CreateFromPrivateKey(AdminPrivateKey, Mode);
        public static PublicAccount AdminPublicAccount = PublicAccount.CreateFromPublicKey(AdminAccount.GetPublicKey(), Mode);
        public const int EpochAdjustment = 1637848847;
        public const string GenerationHash = "7FCCD304802016BEBBCD342A332F91FF1F3BB5E902988B352697BE245F48E836";
        public const string LifeScopedMetadataKey = "D7455FE30716C9F9";
        public const string PowerScopedMetadataKey = "A3DA903C111955FD";
        public const string TestPlayerPublicKey = "AB3D3679097BBD8D8FF268B01A4452793E67D32E8A2A077D72EC85C351D5D948";
    }
}