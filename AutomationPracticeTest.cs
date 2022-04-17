using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using QATest.Setup;
using SeleniumExtras.WaitHelpers;
using System;
using System.Globalization;
using System.Threading;

namespace AutomationPracticeTest
{
    [TestClass]
    public class AutomationPracticeTest
    {
        private TestArguments parameters;
        private readonly string filePath = @"C:\Automation Practice Test Configuration\LogFile.txt";

        [TestInitialize]

        public void Init()
        {
            var downloadDirectory = @"C:\Files";
            var driverDirectory = @"C:\Drivers\";
            var configFilePath = @"C://Automation Practice Test Configuration\config.xml";

            Functions.WriteInto(filePath, "Start of init");
            parameters = new TestArguments(configFilePath);
            Driver.Initiliaze(driverDirectory, downloadDirectory, parameters.Browser);
            Functions.WriteInto(filePath, "End of init");

        }

        [TestMethod]
        public void SearchArticle_AddToCart_MakePayment()
        {
            // set current thread culture to US English for parsing string into decimals later in the code.
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Url.GoTo(parameters.Url);

            var search = Driver.Instance.FindElement(By.Id("search_query_top"));
            search.SendKeys("dress\n"); //insert word 'dress' and press enter
            
            // sort found list by name ascending
            var sort = Driver.Instance.FindElement(By.Id("selectProductSort"));
            var sortElement = new SelectElement(sort);
            sortElement.SelectByValue("name:asc");
  
            // First item in the sorted list is one for buying. Button for adding to cart is only visible on hover on element.
            // Because of that using IJavaScriptExecutor interface style was set to element to always be visible 'display: block'
            var container = Driver.Instance.FindElement(By.CssSelector("#center_column > ul > li:nth-child(1) > div > div.right-block > div.button-container"));
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Driver.Instance;
            executor.ExecuteScript("arguments[0].setAttribute('style', 'display: block')", container);

            // Find button add to cart and execute it.
            var addToCart = Driver.Instance.FindElement(By.CssSelector("#center_column > ul > li:nth-child(1) > div > div.right-block > div.button-container > a.button.ajax_add_to_cart_button.btn.btn-default"));
            addToCart.SendKeys("\n");

            // Adding to cart is async operation. Because of that driver should wait for element to be visible or max 5 seconds. 
            var wait = new WebDriverWait(Driver.Instance, TimeSpan.FromSeconds(5));

            //After element is visible on page proceed to checkout
            var proceedtoCheckOut =wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#layer_cart > div.clearfix > div.layer_cart_cart.col-xs-12.col-md-6 > div.button-container > a")));
            proceedtoCheckOut.Click();

            // Chanhge quantity of selected item.
            int quantityValue = 2;
            var quantity =wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("/html/body/div/div[2]/div/div[3]/div/div[2]/table/tbody/tr/td[5]/input[2]")));
            quantity.Clear();
            quantity.SendKeys(quantityValue.ToString());

            //After chaniging quantity total price should be updated which is also async operation.
            // This time code is terminated for 5 seconds using Thread.Sleep()
            Thread.Sleep(5000);

            // find price per unit, shipping price and total price, conver them to decimal values, and check if total price is updated correctly
            var unitPriceStr = Driver.Instance.FindElement(By.XPath("/html/body/div/div[2]/div/div[3]/div/div[2]/table/tbody/tr/td[4]/span/span")).GetAttribute("textContent");
            var totalShippingStr = Driver.Instance.FindElement(By.Id("total_shipping")).GetAttribute("textContent");
            var newTotalPriceStr = Driver.Instance.FindElement(By.Id("total_price")).GetAttribute("textContent");
            var unitPrice = decimal.Parse(unitPriceStr.Replace("$", ""));
            var totalShipping = decimal.Parse(totalShippingStr.Replace("$", ""));
            var newTotalPrice = decimal.Parse(newTotalPriceStr.Replace("$", ""));
            Assert.AreEqual(newTotalPrice, unitPrice * quantityValue + totalShipping);

            // Proceed to checkout
            Driver.Instance.FindElement(By.CssSelector("a[class='button btn btn-default standard-checkout button-medium']")).Click();

            // sing in with existing account
            var emailAdress = Driver.Instance.FindElement(By.Id("email"));
            emailAdress.SendKeys("nejrarnaut@gmail.com");
            var password=Driver.Instance.FindElement(By.Id("passwd")); 
            password.SendKeys("nejra!!!"); // this should be hidden from code using some external file
            Driver.Instance.FindElement(By.Id("SubmitLogin")).Click();

            // proceed to checkout 
            // added comment to order
            var ordedComment = Driver.Instance.FindElement(By.Id("ordermsg")).FindElement(By.TagName("textarea"));
            ordedComment.SendKeys("I would like to receive this order as soon as possible");
            Driver.Instance.FindElement(By.CssSelector("button[class='button btn btn-default button-medium']")).Click();

            // Check agreement checkbox and proceed to payment
            Driver.Instance.FindElement(By.Id("cgv")).Click();
            Driver.Instance.FindElement(By.CssSelector("button[class='button btn btn-default standard-checkout button-medium']")).Click();
            
            // choose payment type (bankwire)
            var payment = Driver.Instance.FindElement(By.Id("HOOK_PAYMENT")).FindElement(By.ClassName("bankwire"));
            payment.Click();

            // confirm payment
            var confirmPayment = Driver.Instance.FindElement(By.Id("cart_navigation")).FindElement(By.TagName("button"));
            confirmPayment.Click();

            // check if cart is empty after payment
            var cart = Driver.Instance.FindElement(By.ClassName("shopping_cart")).FindElement(By.TagName("a"));
            cart.Click();
            var warningMessage = Driver.Instance.FindElement(By.Id("center_column")).FindElement(By.CssSelector("p[class='alert alert-warning']"));
            Assert.AreEqual(warningMessage.GetAttribute("textContent"), "Your shopping cart is empty.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Driver.Close();
        }
    }
}
