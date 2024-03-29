﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.Payments.Audit.AcceptanceTests.DataLock
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Clean Up Data Lock Events On Submission Success")]
    public partial class CleanUpDataLockEventsOnSubmissionSuccessFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Clean Up Data Lock Events On Submission Success.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Clean Up Data Lock Events On Submission Success", "\tIn order to generate data match report\r\n\tI want to the Payments V2 service to cl" +
                    "ear all old submissions", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Submission Succeeds and Data Lock Events Cleared")]
        public virtual void SubmissionSucceedsAndDataLockEventsCleared()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Submission Succeeds and Data Lock Events Cleared", null, ((string[])(null)));
#line 5
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Submission Time",
                        "Learner",
                        "Failure"});
            table1.AddRow(new string[] {
                        "2019-09-18",
                        "a",
                        "true"});
            table1.AddRow(new string[] {
                        "2019-09-18",
                        "b",
                        "false"});
            table1.AddRow(new string[] {
                        "2019-09-19",
                        "c",
                        "true"});
            table1.AddRow(new string[] {
                        "2019-09-19",
                        "d",
                        "false"});
            table1.AddRow(new string[] {
                        "2019-09-20",
                        "e",
                        "true"});
#line 6
 testRunner.Given("the data lock service has generated the following events", ((string)(null)), table1, "Given ");
#line 13
 testRunner.When("submission succeeded event for ILR submitted at \'2019-09-20\' arrives", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Submission Time",
                        "Learner",
                        "Failure"});
            table2.AddRow(new string[] {
                        "2019-09-20",
                        "e",
                        "true"});
#line 14
 testRunner.Then("only the following events stay in database", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
