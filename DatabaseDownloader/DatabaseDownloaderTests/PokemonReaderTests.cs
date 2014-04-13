using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseDownloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DatabaseDownloader.Tests
{
    [TestClass()]
    public class PokemonReaderTests
    {
        private static string abraName = "Abra";
        private PokemonReader abraReader;
        private static string abraString =
"63\r\nAbra\r\n1\r\n19.5\r\n0.9\r\nPsychic null\r\nSynchronize, Inner Focus, Magic Guard\r\nnull\r\n";
        private static string abraSql =
"INSERT INTO PMOLENDA.POKEMON VALUES (\r\n63,\r\n'Abra',\r\n1,\r\n'19.5',\r\n'0.9',\r\n'Psychic', NULL,\r\n'Synchronize', 'Inner Focus',\r\n'Magic Guard',\r\nNULL);\r\n";

        public PokemonReaderTests()
        {
            abraReader = new PokemonReader.Factory().ReadFromWebPage(abraName);
        }

        [TestMethod()]
        public void PokemonReaderTest()
        {
            Assert.AreEqual(abraReader.Name, abraName);
        }

        [TestMethod()]
        public void GetSqlInsertTest()
        {
            Assert.AreEqual(abraSql, abraReader.GetSqlInsert());
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Assert.AreEqual(abraString, abraReader.ToString()); ;
        }

        //[TestMethod()]
        //public void EvolutionsOrderKeyTest()
        //{
        //    Assert.Fail();
        //}
    }
}
