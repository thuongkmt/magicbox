using System;
using TechTalk.SpecFlow;

namespace KonbiCloud.Specs
{
    [Binding]
    public class MachineManagerSteps
    {
        [Given(@"I know new machine name")]
        public void GivenIKnowNewMachineName()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"I know machine id")]
        public void GivenIKnowMachineId()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the system should add new machine in cloud")]
        public void ThenTheSystemShouldAddNewMachineInCloud()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
