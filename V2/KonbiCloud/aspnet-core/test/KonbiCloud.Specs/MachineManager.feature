Feature: MachineManager
	In order to manage a machine
	As a admin user
	I want to manage machines in cloud

@machine_add
Scenario: Add new machine
	Given I know new machine name
	And I know machine id
	When I press add
	Then the system should add new machine in cloud
