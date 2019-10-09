/// <reference types="Cypress" />

describe('Toss Full Test', function () {
    let polyfill;

    before(() => {
        const polyfillUrl = 'https://unpkg.com/whatwg-fetch@3.0.0/dist/fetch.umd.js';
        cy.request(polyfillUrl).then(response => {
            polyfill = response.body;
        });
    });
    Cypress.on('window:before:load', win => {
        delete win.fetch;
        win.eval(polyfill);
    });
    const SubscribeEmail = "toss-unittests@yopmail.com";
    const SubscribePassword = "tossUnittests123456!!";
    const SubscribeLogin = "tossunittests";

    it('Full process', function () {
        cy.server();

        cy.visit("/");
        cy.route('POST', '/api/account/register').as('register');
        //this could be lagging as ravendb is starting
        cy.get("#LinkLogin", { timeout: 20000 }).click();
        //register
        cy.get("#LinkRegister").click();
        cy.get("#NewEmail").type(SubscribeEmail);
        cy.get("#NewName").type(SubscribeLogin);
        cy.get("#NewPassword").type(SubscribePassword);
        cy.get("#NewConfirmPassword").type(SubscribePassword);
        cy.get("#BtnRegister").click();
        cy.wait('@register', { timeout: 20000 });
        cy.get('@register').then(function (xhr) {
            expect(xhr.status).to.eq(200);
        });

        // //validate subscription
        // var confirmationLink = _serverFixture.EmailSender.confirmationLinks.First(l => l.email == SubscribeEmail).link;
        // Browser.Navigate().GoToUrl(confirmationLink);
        // DisableRecaptcha();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/login"));

        // //log in
        // Browser.FindElement(By.Id("UserName")).SendKeys(SubscribeEmail);
        // Browser.FindElement(By.Id("Password")).SendKeys(SubscribePassword);
        // Browser.FindElement(By.Id("BtnLogin")).Click();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/"));

        // //publish toss
        // Browser.FindElement(By.Id("LinkNewToss")).Click();
        // _webDriveWaitDefault.Until(b => b.FindElement(By.Id("TxtNewToss")).Displayed);
        // string newTossContent = @"lorem ipsum lorem ipsumlorem ipsum lorem ipsumlorem ipsum lorem ipsumlorem ipsum lorem ipsum #test";
        // Browser.FindElement(By.Id("TxtNewToss")).SendKeys(newTossContent);
        // Browser.FindElement(By.Id("BtnNewToss")).Click();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/"));

        // //add new toss x 2
        // Browser.FindElement(By.Id("LinkNewToss")).Click();
        // _webDriveWaitDefault.Until(b => b.FindElement(By.Id("TxtNewToss")).Displayed);
        // Browser.FindElement(By.Id("TxtNewToss")).SendKeys(@" lorem ipsum lorem ipsumlorem ipsum lorem ipsumlorem ipsum  lorem ipsumlorem ipsum lorem ipsum #toto");
        // Browser.FindElement(By.Id("BtnNewToss")).Click();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/"));

        // //add new hashtag
        // Browser.FindElement(By.Id("TxtAddHashTag")).SendKeys(@"test");
        // Browser.FindElement(By.Id("TxtAddHashTag")).SendKeys(Environment.NewLine);
        // Browser.FindElement(By.Id("BtnAddHashTag")).Click();
        // _webDriveWaitDefault.Until(b => b.FindElements(By.CssSelector(".tag-link")).Any());

        // //filter on hashtag
        // Browser.FindElement(By.CssSelector(".tag-link")).Click();

        // // read first toss
        // _webDriveWaitDefault.Until(b => b.FindElements(By.CssSelector(".toss-preview")).Any());
        // Browser.FindElement(By.CssSelector(".toss-preview")).Click();
        // _webDriveWaitDefault.Until(b => b.FindElement(By.CssSelector(".toss-detail .toss-content")).Text == newTossContent);

        // //sign out
        // Browser.FindElement(By.Id("LinkAccount")).Click();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/account"));
        // ScrollToView(By.Id("BtnLogout"));
        // Browser.FindElement(By.Id("BtnLogout")).Click();
        // _webDriveWaitDefault.Until(b => b.Url.EndsWith("/login"));
    })
})