using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Symnity.Http.Model
{
    [Serializable]
    public class ApiNetwork : MonoBehaviour
    {
        public static async UniTask<NetworkRoot> GetTheNetworkProperties(string node)
        {
            const string url = "/network/properties";
            var networkRootStr = await HttpUtiles.GetDataFromApiString(node, url);
            var networkRoot = JsonUtility.FromJson<NetworkRoot>(networkRootStr);
            return networkRoot;
        }

        public class Network
        {
            public string identifier;
            public string nemesisSignerPublicKey;
            public string nodeEqualityStrategy;
            public string generationHashSeed;
            public string epochAdjustment;
        }

        public class Chain
        {
            public bool enableVerifiableState;
            public bool enableVerifiableReceipts;
            public string currencyMosaicId;
            public string harvestingMosaicId;
            public string blockGenerationTargetTime;
            public string blockTimeSmoothingFactor;
            public string importanceGrouping;
            public string importanceActivityPercentage;
            public string maxRollbackBlocks;
            public string maxDifficultyBlocks;
            public string defaultDynamicFeeMultiplier;
            public string maxTransactionLifetime;
            public string maxBlockFutureTime;
            public string initialCurrencyAtomicUnits;
            public string maxMosaicAtomicUnits;
            public string totalChainImportance;
            public string minHarvesterBalance;
            public string maxHarvesterBalance;
            public string minVoterBalance;
            public string votingSetGrouping;
            public string maxVotingKeysPerAccount;
            public string minVotingKeyLifetime;
            public string maxVotingKeyLifetime;
            public string harvestBeneficiaryPercentage;
            public string harvestNetworkPercentage;
            public string harvestNetworkFeeSinkAddressV1;
            public string harvestNetworkFeeSinkAddress;
            public string maxTransactionsPerBlock;
        }

        public class Accountlink
        {
            public string dummy;
        }

        public class Aggregate
        {
            public string maxTransactionsPerAggregate;
            public string maxCosignaturesPerAggregate;
            public bool enableStrictCosignatureCheck;
            public bool enableBondedAggregateSupport;
            public string maxBondedTransactionLifetime;
        }

        public class Lockhash
        {
            public string lockedFundsPerAggregate;
            public string maxHashLockDuration;
        }

        public class Locksecret
        {
            public string maxSecretLockDuration;
            public string minProofSize;
            public string maxProofSize;
        }

        public class Metadata
        {
            public string maxValueSize;
        }

        public class Mosaic
        {
            public string maxMosaicsPerAccount;
            public string maxMosaicDuration;
            public string maxMosaicDivisibility;
            public string mosaicRentalFeeSinkAddressV1;
            public string mosaicRentalFeeSinkAddress;
            public string mosaicRentalFee;
        }

        public class Multisig
        {
            public string maxMultisigDepth;
            public string maxCosignatoriesPerAccount;
            public string maxCosignedAccountsPerAccount;
        }

        public class Namespace
        {
            public string maxNameSize;
            public string maxChildNamespaces;
            public string maxNamespaceDepth;
            public string minNamespaceDuration;
            public string maxNamespaceDuration;
            public string namespaceGracePeriodDuration;
            public string reservedRootNamespaceNames;
            public string namespaceRentalFeeSinkAddressV1;
            public string namespaceRentalFeeSinkAddress;
            public string rootNamespaceRentalFeePerBlock;
            public string childNamespaceRentalFee;
        }

        public class Restrictionaccount
        {
            public string maxAccountRestrictionValues;
        }

        public class Restrictionmosaic
        {
            public string maxMosaicRestrictionValues;
        }

        public class Transfer
        {
            public string maxMessageSize;
        }

        public class Plugins
        {
            public Accountlink accountlink;
            public Aggregate aggregate;
            public Lockhash lockhash;
            public Locksecret locksecret;
            public Metadata metadata;
            public Mosaic mosaic;
            public Multisig multisig;
            public Namespace @namespace;
            public Restrictionaccount restrictionaccount;
            public Restrictionmosaic restrictionmosaic;
            public Transfer transfer;
        }

        public class NetworkRoot
        {
            public Network network;
            public Chain chain;
            public Plugins plugins;
        }
    }
}