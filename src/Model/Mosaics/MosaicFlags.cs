using System;
using System.Collections.Generic;

namespace Symnity.Model.Mosaics
{
    [Serializable]
    /*
     * Mosaic flags model
     */
    public class MosaicFlags
    {
        /**
         * The creator can choose between a definition that allows a mosaic supply change at a later point or an immutable supply.
         * Allowed values for the property are "true" and "false". The default value is "false".
         */
        public readonly bool SupplyMutable;

        /**
         * The creator can choose if the mosaic definition should allow for transfers of the mosaic among accounts other than the creator.
         * If the property 'transferable' is set to "false", only transfer transactions
         * having the creator as sender or as recipient can transfer mosaics of that type.
         * If set to "true" the mosaics can be transferred to and from arbitrary accounts.
         * Allowed values for the property are thus "true" and "false". The default value is "true".
         */
        public readonly bool Transferable;

        /**
         * Not all the mosaics of a given network will be subject to mosaic restrictions. The feature will only affect
         * those to which the issuer adds the "restrictable" property explicitly at the moment of its creation. This
         * property appears disabled by default, as it is undesirable for autonomous tokens like the public network currency.
         */
        public readonly bool Restrictable;
        
        /**
         *  The creator can choose if he can revoke tokens after a transfer.
         */
        public readonly bool Revokable;

        /*
         * @param flags
         * @param divisibility
         * @param duration
         */
        MosaicFlags(bool supplyMutable, bool transferable, bool restrictable = false, bool revokable = false)
        {
            SupplyMutable = supplyMutable;
            Transferable = transferable;
            Restrictable = restrictable;
            Revokable = revokable;
        }

        /**
             * Static constructor function with default parameters
             * @returns {MosaicFlags}
             * @param supplyMutable
             * @param transferable
             * @param restrictable
             */
        public static MosaicFlags Create(bool supplyMutable, bool transferable, bool restrictable = false, bool revokable = false)
        {
            return new MosaicFlags( supplyMutable, transferable, restrictable, revokable);
        }

        /**
        * Get mosaic flag value in number
        * @returns {number}
        */
        public int GetValue()
        {
            return (SupplyMutable ? 1 : 0) + (Transferable ? 2 : 0) + (Restrictable ? 4 : 0) + (Revokable ? 8 : 0);
        }
    }
}