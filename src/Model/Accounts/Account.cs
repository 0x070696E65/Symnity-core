using System;
using Symnity.Core.Crypto;
using Symnity.Core.Format;
using Symnity.Model.Network;
using Symnity.Model.Transactions;

namespace Symnity.Model.Accounts
{
    /**
    * The account structure describes an account private key, public key, address and allows signing transactions.
    */
    [Serializable]
    public class Account
    {
        /*
         * @internal
         * @param address
         * @param keyPair
         */
        /**
         * The account address.
         */
        public readonly Address Address;

        /**
         * The account keyPair, public and private key.
         */
        public readonly KeyPair _keyPair;

        /**
         * Create an Account from a given private key
         * @param privateKey - Private key from an account
         * @param networkType - Network type
         * @return {Account}
         */
        public Account(Address address, KeyPair keyPair)
        {
            Address = address;
            _keyPair = keyPair;
        }
        
        /**
         * Create an Account from a given private key
         * @param privateKey - Private key from an account
         * @param networkType - Network type
         * @return {Account}
         */
        public static Account CreateFromPrivateKey(string privateKey, NetworkType networkType ) {
            var keyPair = KeyPair.CreateKeyPairFromPrivateKeyString(privateKey);
            var address = RawAddress.AddressToString(RawAddress.PublicKeyToAddress(keyPair.publicKey, networkType));
            return new Account(Address.CreateFromRawAddress(address), keyPair);
        }
        
        /**
         * Generate a new account
         * @param networkType - Network type
         */
        public static Account GenerateNewAccount(NetworkType networkType) {
            // Create random bytes
            var randomBytesArray = Crypto.RandomBytes(32);
            // Hash random bytes with entropy seed
            // Finalize and keep only 32 bytes
            var hashKey = ConvertUtils.ToHex(randomBytesArray);

            // Create KeyPair from hash key
            var keyPair = KeyPair.CreateKeyPairFromPrivateKeyString(hashKey);

            var address = Address.CreateFromPublicKey(ConvertUtils.ToHex(keyPair.publicKey), networkType);
            return new Account(address, keyPair);
        }
        
        /**
         * Account public key.
         * @return {string}
         */
        public string GetPublicKey() {
            return ConvertUtils.ToHex(_keyPair.publicKey);
        }
        
        /**
        * Public account.
        * @return {PublicAccount}
        */
        public PublicAccount GetPublicAccount() {
            return PublicAccount.CreateFromPublicKey(GetPublicKey(), Address.NetworkType);
        }
        
        /**
         * Account private key.
         * @return {string}
         */
        public string GetPrivateKey() {
            return ConvertUtils.ToHex(_keyPair.privateKey);
        }
        
        /**
         * Sign a transaction
         * @param transaction - The transaction to be signed.
         * @param generationHash - Network generation hash hex
         * @return {SignedTransaction}
         */
        public SignedTransaction Sign(Transaction transaction, string generationHash) {
            return transaction.SignWith(this, generationHash);
        }
        
        /**
         * Sign transaction with cosignatories collected from cosigned transactions and creating a new SignedTransaction
         * For off chain Aggregated Complete Transaction co-signing.
         * @param initiatorAccount - Initiator account
         * @param {CosignatureSignedTransaction[]} cosignatureSignedTransactions - Array of cosigned transaction
         * @param generationHash - Network generation hash hex
         * @return {SignedTransaction}
         */
        public SignedTransaction SignTransactionGivenSignatures(
            AggregateTransaction transaction,
            CosignatureSignedTransaction[] cosignatureSignedTransactions,
            string generationHash
        ) {
            return transaction.SignTransactionGivenSignatures(this, cosignatureSignedTransactions, generationHash);
        }
    }
}