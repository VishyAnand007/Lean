/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using QuantConnect.Securities;
using QuantConnect.Securities.Crypto;
using QuantConnect.Securities.Forex;

namespace QuantConnect.Util
{
    /// <summary>
    /// Utility methods for decomposing and comparing currency pairs
    /// </summary>
    public static class CurrencyPairUtil
    {
        private static readonly Lazy<SymbolPropertiesDatabase> SymbolPropertiesDatabase =
            new Lazy<SymbolPropertiesDatabase>(Securities.SymbolPropertiesDatabase.FromDataFolder);

        /// <summary>
        /// Decomposes the specified currency pair into a base and quote currency provided as out parameters
        /// </summary>
        /// <param name="currencyPair">The input currency pair to be decomposed</param>
        /// <param name="baseCurrency">The output base currency</param>
        /// <param name="quoteCurrency">The output quote currency</param>
        public static void DecomposeCurrencyPair(Symbol currencyPair, out string baseCurrency, out string quoteCurrency)
        {
            if (currencyPair == null)
            {
                throw new ArgumentException("Currency pair must not be null");
            }

            if (currencyPair.SecurityType == SecurityType.Crypto)
            {
                var symbolProperties = SymbolPropertiesDatabase.Value.GetSymbolProperties(
                    currencyPair.ID.Market,
                    currencyPair,
                    currencyPair.SecurityType,
                    Currencies.USD);

                Crypto.DecomposeCurrencyPair(currencyPair, symbolProperties, out baseCurrency, out quoteCurrency);
            }
            else
            {
                Forex.DecomposeCurrencyPair(currencyPair.Value, out baseCurrency, out quoteCurrency);
            }
        }

        /// <summary>
        /// You have currencyPair AB and one known symbol (A or B). This function returns the other symbol (B or A).
        /// </summary>
        /// <param name="currencyPair">Currency pair AB</param>
        /// <param name="knownSymbol">Known part of the currencyPair (either A or B)</param>
        /// <returns>The other part of currencyPair (either B or A)</returns>
        public static string CurrencyPairDual(this Symbol currencyPair, string knownSymbol)
        {
            string currencyA;
            string currencyB;

            DecomposeCurrencyPair(currencyPair, out currencyA, out currencyB);

            if (currencyA == knownSymbol)
            {
                return currencyB;
            }

            if (currencyB == knownSymbol)
            {
                return currencyA;
            }

            throw new ArgumentException(
                $"The known symbol {knownSymbol} isn't contained in currency pair {currencyPair}.");
        }

        /// <summary>
        /// Represents the relation between two currency pairs
        /// </summary>
        public enum Match
        {
            /// <summary>
            /// The two currency pairs don't match each other normally nor when one is reversed
            /// </summary>
            NoMatch,

            /// <summary>
            /// The two currency pairs match each other exactly
            /// </summary>
            ExactMatch,

            /// <summary>
            /// The two currency pairs are the inverse of each other
            /// </summary>
            InverseMatch
        }

        /// <summary>
        /// Returns how two currency pairs are related to each other
        /// </summary>
        /// <param name="pairA">The first pair</param>
        /// <param name="pairB">The second pair</param>
        /// <returns>The <see cref="Match"/> member that represents the relation between the two pairs</returns>
        public static Match ComparePair(this Symbol pairA, Symbol pairB)
        {
            if (pairA == pairB)
            {
                return Match.ExactMatch;
            }

            string baseCurrencyA;
            string quoteCurrencyA;

            DecomposeCurrencyPair(pairA, out baseCurrencyA, out quoteCurrencyA);

            string baseCurrencyB;
            string quoteCurrencyB;

            DecomposeCurrencyPair(pairB, out baseCurrencyB, out quoteCurrencyB);

            if (baseCurrencyA == quoteCurrencyB && baseCurrencyB == quoteCurrencyA)
            {
                return Match.InverseMatch;
            }

            return Match.NoMatch;
        }

        /// <summary>
        /// Returns how two currency pairs are related to each other
        /// </summary>
        /// <param name="pairA">The first pair</param>
        /// <param name="baseCurrencyB">The base currency of the second pair</param>
        /// <param name="quoteCurrencyB">The quote currency of the second pair</param>
        /// <returns>The <see cref="Match"/> member that represents the relation between the two pairs</returns>
        public static Match ComparePair(this Symbol pairA, string baseCurrencyB, string quoteCurrencyB)
        {
            if (pairA.Value == baseCurrencyB + quoteCurrencyB)
            {
                return Match.ExactMatch;
            }

            string baseCurrencyA;
            string quoteCurrencyA;

            DecomposeCurrencyPair(pairA, out baseCurrencyA, out quoteCurrencyA);

            if (baseCurrencyA == quoteCurrencyB && baseCurrencyB == quoteCurrencyA)
            {
                return Match.InverseMatch;
            }

            return Match.NoMatch;
        }

        /// <summary>
        /// Returns whether a currency pair contains a certain currency as base or as quote
        /// </summary>
        /// <param name="pair">The currency pair to check the sides of</param>
        /// <param name="currency">The currency to look for</param>
        /// <returns>True if currency is the base or quote currency of pair, false if not</returns>
        public static bool PairContainsCurrency(this Symbol pair, string currency)
        {
            string baseCurrency;
            string quoteCurrency;

            DecomposeCurrencyPair(pair, out baseCurrency, out quoteCurrency);

            return baseCurrency == currency || quoteCurrency == currency;
        }
    }
}
