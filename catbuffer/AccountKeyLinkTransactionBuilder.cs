/**
*** Copyright (c) 2016-2019, Jaguar0625, gimre, BloodyRookie, Tech Bureau, Corp.
*** Copyright (c) 2020-present, Jaguar0625, gimre, BloodyRookie.
*** All rights reserved.
***
*** This file is part of Catapult.
***
*** Catapult is free software: you can redistribute it and/or modify
*** it under the terms of the GNU Lesser General Public License as published by
*** the Free Software Foundation, either version 3 of the License, or
*** (at your option) any later version.
***
*** Catapult is distributed in the hope that it will be useful,
*** but WITHOUT ANY WARRANTY; without even the implied warranty of
*** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
*** GNU Lesser General Public License for more details.
***
*** You should have received a copy of the GNU Lesser General Public License
*** along with Catapult. If not, see <http://www.gnu.org/licenses/>.
**/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Symbol.Builders {
    /*
    * This transaction is required for all accounts wanting to activate remote or delegated harvesting.
Announce an AccountKeyLinkTransaction to delegate the account importance score to a proxy account.
    */
    [Serializable]
    public class AccountKeyLinkTransactionBuilder: TransactionBuilder {

        /* Account key link transaction body. */
        public AccountKeyLinkTransactionBodyBuilder accountKeyLinkTransactionBody;
        
        /*
        * Constructor - Creates an object from stream.
        *
        * @param stream Byte stream to use to serialize the object.
        */
        internal AccountKeyLinkTransactionBuilder(BinaryReader stream)
            : base(stream)
        {
            try {
                accountKeyLinkTransactionBody = AccountKeyLinkTransactionBodyBuilder.LoadFromBinary(stream);
            } catch (Exception e) {
                throw new Exception(e.ToString());
            }
        }

        /*
        * Creates an instance of AccountKeyLinkTransactionBuilder from a stream.
        *
        * @param stream Byte stream to use to serialize the object.
        * @return Instance of AccountKeyLinkTransactionBuilder.
        */
        public new static AccountKeyLinkTransactionBuilder LoadFromBinary(BinaryReader stream) {
            return new AccountKeyLinkTransactionBuilder(stream);
        }

        
        /*
        * Constructor.
        *
        * @param signature Entity's signature generated by the signing account..
        * @param signerPublicKey Public key of the signer of the entity..
        * @param version Version of this structure..
        * @param network Network on which this entity was created..
        * @param type Transaction type.
        * @param fee Transaction fee.
        * @param deadline Transaction deadline.
        * @param linkedPublicKey Linked public key..
        * @param linkAction Account link action..
        */
        internal AccountKeyLinkTransactionBuilder(SignatureDto signature, PublicKeyDto signerPublicKey, byte version, NetworkTypeDto network, TransactionTypeDto type, AmountDto fee, TimestampDto deadline, PublicKeyDto linkedPublicKey, LinkActionDto linkAction)
            : base(signature, signerPublicKey, version, network, type, fee, deadline)
        {
            GeneratorUtils.NotNull(signature, "signature is null");
            GeneratorUtils.NotNull(signerPublicKey, "signerPublicKey is null");
            GeneratorUtils.NotNull(version, "version is null");
            GeneratorUtils.NotNull(network, "network is null");
            GeneratorUtils.NotNull(type, "type is null");
            GeneratorUtils.NotNull(fee, "fee is null");
            GeneratorUtils.NotNull(deadline, "deadline is null");
            GeneratorUtils.NotNull(linkedPublicKey, "linkedPublicKey is null");
            GeneratorUtils.NotNull(linkAction, "linkAction is null");
            this.accountKeyLinkTransactionBody = new AccountKeyLinkTransactionBodyBuilder(linkedPublicKey, linkAction);
        }
        
        /*
        * Creates an instance of AccountKeyLinkTransactionBuilder.
        *
        * @param signature Entity's signature generated by the signing account..
        * @param signerPublicKey Public key of the signer of the entity..
        * @param version Version of this structure..
        * @param network Network on which this entity was created..
        * @param type Transaction type.
        * @param fee Transaction fee.
        * @param deadline Transaction deadline.
        * @param linkedPublicKey Linked public key..
        * @param linkAction Account link action..
        * @return Instance of AccountKeyLinkTransactionBuilder.
        */
        public static  AccountKeyLinkTransactionBuilder Create(SignatureDto signature, PublicKeyDto signerPublicKey, byte version, NetworkTypeDto network, TransactionTypeDto type, AmountDto fee, TimestampDto deadline, PublicKeyDto linkedPublicKey, LinkActionDto linkAction) {
            return new AccountKeyLinkTransactionBuilder(signature, signerPublicKey, version, network, type, fee, deadline, linkedPublicKey, linkAction);
        }

        /*
        * Gets Linked public key..
        *
        * @return Linked public key..
        */
        public PublicKeyDto GetLinkedPublicKey() {
            return accountKeyLinkTransactionBody.GetLinkedPublicKey();
        }

        /*
        * Gets Account link action..
        *
        * @return Account link action..
        */
        public LinkActionDto GetLinkAction() {
            return accountKeyLinkTransactionBody.GetLinkAction();
        }

    
        /*
        * Gets the size of the object.
        *
        * @return Size in bytes.
        */
        public override int GetSize() {
            var size = base.GetSize();
            size += accountKeyLinkTransactionBody.GetSize();
            return size;
        }

        /*
        * Gets the body builder of the object.
        *
        * @return Body builder.
        */
        public new AccountKeyLinkTransactionBodyBuilder GetBody() {
            return accountKeyLinkTransactionBody;
        }


    
        /*
        * Serializes an object to bytes.
        *
        * @return Serialized bytes.
        */
        public new byte[] Serialize() {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var superBytes = base.Serialize();
            bw.Write(superBytes, 0, superBytes.Length);
            var accountKeyLinkTransactionBodyEntityBytes = (accountKeyLinkTransactionBody).Serialize();
            bw.Write(accountKeyLinkTransactionBodyEntityBytes, 0, accountKeyLinkTransactionBodyEntityBytes.Length);
            var result = ms.ToArray();
            return result;
        }
    }
}