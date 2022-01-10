using System;
using System.Collections.Generic;
using Symbol.Builders;
using Symnity.Core.Format;
using Symnity.Model.Accounts;
using Symnity.Model.BlockChain;
using Symnity.Model.Mosaics;
using Symnity.Model.Network;
using Symnity.Model;

namespace Symnity.Model.Transactions
{
    public class MosaicDefinitionTransaction : Transaction
    {
        /**
         * The mosaic nonce.
         */
        public readonly MosaicNonce nonce;

        /**
         * The mosaic id.
         */
        public readonly MosaicId mosaicId;

        /**
         * The mosaic properties.
         */
        public readonly MosaicFlags flags;

        /**
         * Mosaic divisibility
         */
        public readonly byte divisibility;

        /**
         * Mosaic duration, 0 value for eternal mosaic
         */
        public readonly BlockDuration duration;

        private string signature;
        private PublicAccount signer;
        private TransactionInfo transactionInfo;
        
        /**
         * Create a mosaic creation transaction object
         * @param deadline - The deadline to include the transaction.
         * @param nonce - The mosaic nonce ex: MosaicNonce.createRandom().
         * @param mosaicId - The mosaic id ex: new MosaicId([481110499, 231112638]).
         * @param flags - The mosaic flags.
         * @param divisibility - The mosaic divicibility.
         * @param duration - The mosaic duration.
         * @param networkType - The network type.
         * @param maxFee - (Optional) Max fee defined by the sender
         * @param signature - (Optional) Transaction signature
         * @param signer - (Optional) Signer public account
         * @returns {MosaicDefinitionTransaction}
         */
        public static MosaicDefinitionTransaction Create(
            Deadline deadline,
            MosaicNonce nonce,
            MosaicId mosaicId,
            MosaicFlags flags,
            byte divisibility,
            BlockDuration duration,
            NetworkType networkType,
            long maxFee = 0,
            string signature = null,
            PublicAccount signer= null
        ) {
            return new MosaicDefinitionTransaction(
                networkType,
                TransactionVersion.MOSAIC_DEFINITION,
                deadline,
                maxFee,
                nonce,
                mosaicId,
                flags,
                divisibility,
                duration,
                signature,
                signer
            );
        }
        
        /**
         * @param networkType
         * @param version
         * @param deadline
         * @param maxFee
         * @param nonce
         * @param mosaicId
         * @param flags
         * @param divisibility
         * @param duration
         * @param signature
         * @param signer
         * @param transactionInfo
         */
        public MosaicDefinitionTransaction(
            NetworkType networkType, 
            byte version,
            Deadline deadline, 
            long maxFee,
            MosaicNonce nonce,
            MosaicId mosaicId,
            MosaicFlags flags,
            byte divisibility,
            BlockDuration duration,
            string signature = null,
            PublicAccount signer = null,
            TransactionInfo transactionInfo = null
        )
        : base(TransactionType.MOSAIC_DEFINITION, networkType, version, deadline, maxFee, signature, signer, transactionInfo)
        {
            this.nonce = nonce;
            this.mosaicId = mosaicId;
            this.flags = flags;
            this.divisibility = divisibility;
            this.duration = duration;
        }

        /**
        * @internal
        * @returns {byte[]}
        */
        protected override byte[] GenerateBytes()
        {
            return CreateBuilder().Serialize();
        }
        
        /**
         * @description Serialize a transaction object
         * @returns {string}
         * @memberof Transaction
         */
        public override string Serialize()
        {
            return ConvertUtils.ToHex(GenerateBytes());
        }
        
        /**
         * @internal
         * @returns {TransactionBuilder}
         */
        public new MosaicDefinitionTransactionBuilder CreateBuilder()
        {
            return new MosaicDefinitionTransactionBuilder(
                GetSignatureAsBuilder(),
                GetSignerAsBuilder(),
                VersionToDTO(),
                (NetworkTypeDto)Enum.ToObject(typeof(NetworkTypeDto), (byte) NetworkType),
                (TransactionTypeDto)Enum.ToObject(typeof(TransactionTypeDto), (short) TransactionType.MOSAIC_DEFINITION),
                new AmountDto(MaxFee),
                new TimestampDto(Deadline.AdjustedValue),
                new MosaicIdDto(mosaicId.GetIdAsLong()),
                new BlockDurationDto(duration.GetDuration()),
                new MosaicNonceDto(nonce.nonce),
                GeneratorUtils.ToSet<MosaicFlagsDto>(flags.GetValue()),
                divisibility
            );
        }
        
        /**
         * @internal
         * @returns {EmbeddedTransactionBuilder}
         */
        public override EmbeddedTransactionBuilder ToEmbeddedTransaction() {
            return new EmbeddedMosaicDefinitionTransactionBuilder(
                GetSignerAsBuilder(),
                VersionToDTO(),
                (NetworkTypeDto)Enum.ToObject(typeof(NetworkTypeDto), (byte) NetworkType),
                (TransactionTypeDto)Enum.ToObject(typeof(TransactionTypeDto), (short) TransactionType.MOSAIC_DEFINITION),
                new MosaicIdDto(mosaicId.GetIdAsLong()),
                new BlockDurationDto(duration.GetDuration()),
                new MosaicNonceDto(nonce.nonce),
                GeneratorUtils.ToSet<MosaicFlagsDto>(flags.GetValue()),
                divisibility
            );
        }
    }
}