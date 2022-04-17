# AutomationPracticeTest
In this smoke test I used Selenium Automation framework with C# in Chrome environment.
For this packet I used nuget packege [QATest.Setup](https://github.com/nejraacelik/QATest.Setup) which is also made by me.

Testing of website automationpractice.com in following way:
* Open website.
* Find search field and enter 'dress' as search query. 
* When results are displayed, sort using filter product name: A-Z. 
* Then choose one of offered option and add to cart (first one from the list). 
* After that quantity was changed and checked total price.
* Proceeded to sign in using email adress and password already registered.
* Confirm my adress and payment method. 
* At the end checked my cart is empty, by returning to card page and checking message that is displayed there.


