﻿using Microsoft.Research.SEAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SEALNetTest
{
    [TestClass]
    public class EncryptionParametersTests
    {
        [TestMethod]
        public void CreateTest()
        {
            EncryptionParameters encParams = new EncryptionParameters(SchemeType.BFV);

            Assert.IsNotNull(encParams);
            Assert.AreEqual(SchemeType.BFV, encParams.Scheme);

            EncryptionParameters encParams2 = new EncryptionParameters(SchemeType.CKKS);

            Assert.IsNotNull(encParams2);
            Assert.AreEqual(SchemeType.CKKS, encParams2.Scheme);

            EncryptionParameters encParams3 = new EncryptionParameters(SchemeType.CKKS);

            Assert.IsNotNull(encParams3);
            Assert.AreEqual(SchemeType.CKKS, encParams3.Scheme);

            Assert.AreEqual(encParams2.ParmsId, encParams3.ParmsId);
            Assert.AreNotEqual(encParams.ParmsId, encParams2.ParmsId);
        }

        [TestMethod]
        public void CoeffModulusTest()
        {
            EncryptionParameters encParams = new EncryptionParameters(SchemeType.BFV);

            Assert.IsNotNull(encParams);
            Assert.AreEqual(4, encParams.ParmsId.Block.Length);

            List<SmallModulus> coeffs = new List<SmallModulus>(encParams.CoeffModulus);
            Assert.IsNotNull(coeffs);
            Assert.AreEqual(0, coeffs.Count);

            coeffs = new List<SmallModulus>(DefaultParams.CoeffModulus128(4096));
            encParams.CoeffModulus = coeffs;

            List<SmallModulus> newCoeffs = new List<SmallModulus>(encParams.CoeffModulus);
            Assert.IsNotNull(newCoeffs);
            Assert.AreEqual(2, newCoeffs.Count);
            Assert.AreEqual(0x007fffffff380001ul, newCoeffs[0].Value);
            Assert.AreEqual(0x003fffffff000001ul, newCoeffs[1].Value);
        }

        [TestMethod]
        public void SaveLoadTest()
        {
            List<SmallModulus> coeffModulus = new List<SmallModulus>
            {
                DefaultParams.SmallMods40Bit(0),
                DefaultParams.SmallMods40Bit(1)
            };
            EncryptionParameters parms = new EncryptionParameters(SchemeType.BFV)
            {
                PolyModulusDegree = 8,
                PlainModulus = new SmallModulus(257),
                CoeffModulus = coeffModulus
            };

            EncryptionParameters loaded = null;

            using (MemoryStream stream = new MemoryStream())
            {
                EncryptionParameters.Save(parms, stream);

                stream.Seek(offset: 0, loc: SeekOrigin.Begin);

                loaded = EncryptionParameters.Load(stream);
            }

            Assert.AreEqual(SchemeType.BFV, loaded.Scheme);
            Assert.AreEqual(8, loaded.PolyModulusDegree);
            Assert.AreEqual(257ul, loaded.PlainModulus.Value);

            List<SmallModulus> loadedCoeffModulus = new List<SmallModulus>(loaded.CoeffModulus);
            Assert.AreEqual(2, loadedCoeffModulus.Count);
            Assert.AreNotSame(coeffModulus[0], loadedCoeffModulus[0]);
            Assert.AreNotSame(coeffModulus[1], loadedCoeffModulus[1]);
            Assert.AreEqual(coeffModulus[0], loadedCoeffModulus[0]);
            Assert.AreEqual(coeffModulus[1], loadedCoeffModulus[1]);
            Assert.AreEqual(parms.NoiseMaxDeviation, loaded.NoiseMaxDeviation, delta: 0.001);
            Assert.AreEqual(parms.NoiseStandardDeviation, loaded.NoiseStandardDeviation, delta: 0.001);
        }
    }
}
