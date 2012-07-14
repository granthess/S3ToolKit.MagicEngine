Feature: Package Management
	In order to manage my Custom Content
	As a CC Magic User
	I want to be able to manage .package files

Scenario: Set Package Description
	Given I have a package selected
	And I enter a description "Package Description"
	When I apply changes
	Then the package description should be "Package Description"
