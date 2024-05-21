using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace SACLERP.CurencyOperation
{
    public class BaseClass
    {
        private static List<CurrencyInfo> currencies = new List<CurrencyInfo>();

        public BaseClass()
        {
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Syria));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.UAE));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.SaudiArabia));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Tunisia));
            currencies.Add(new CurrencyInfo(CurrencyInfo.Currencies.Gold));
        }

        public string ChangeNumberToText(string txtNumber, int currencyId)
        {
            string txtEnglishWord, txtArabicWord;
            try
            {
                ToWord toWord = new ToWord(Convert.ToDecimal(txtNumber), currencies[currencyId]);
                txtEnglishWord = toWord.ConvertToEnglish();
                txtArabicWord = toWord.ConvertToArabic();
            }
            catch (Exception ex)
            {
                txtEnglishWord = String.Empty;
                txtArabicWord = String.Empty;
            }

            return txtArabicWord;
        }
    }
}