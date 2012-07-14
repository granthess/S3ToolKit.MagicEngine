Feature: Package Management
	In order to manage my Custom Content
	As a Sims player
	I want to be able to manage .package files

Scenario: Set Package Description
	Given I have a package selected
	And I enter a description "Package Description"
	When I apply changes
	Then the package description should be "Package Description"

Scenario: Add Set
	Given I have an existing set selected
	And I enter a new set name "Set Name"
	When I apply changes
	Then the new set should be created 
	And the new set should be named "Set Name"
	And the existing set should contain the new set "Set Name"
	
Scenario: Remove Set
	Given I have a set selected 
	And that set has a parent set
	When I delete the selected set
	Then the parent set should contain the packages in the selected set
	And the parent set should contain the child sets of the selected set
	And the selected set should be removed from the program
	
Scenario: Add Package to Set
	Given I have selected a package
	And I have selected a set
	When I add the package to the set
	Then the set should contain the package
	
Scenario: Remove Package
	Given I have selected a package
	When I delete the package
	Then the package should be removed from the program

Scenario: Make a Set a Child of Another Set
	Given I have selected a set
	And I have selected another set
	When I place the first set into the second set
	Then the second set should contain the first set
	
Scenario: Disable a Subobject in a Package
	Given I have selected a package
	And I have selected a sub-object in the package
	And the sub-object is enabled
	When I disable the sub-object
	Then the sub-object will be disabled (and appear in game)
	
Scenario: Enable a Subobject in a Package
	Given I have selected a package
	And I have selected a sub-object in the package
	And the sub-object is disabled
	When I enable the sub-object
	Then the sub-object will be enabled (and appear in game)