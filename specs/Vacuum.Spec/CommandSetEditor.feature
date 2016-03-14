@commandSetEditor
Feature: Command Set Editor
	As a user I'd like to be able to edit command sets

Scenario: Adding a new command
	Given user is editing a command set
	When user adds command 'Test Command'
	Then command editor is shown

Scenario: User cannot execute remove without a selected command
	Given user is editing a command set
	Given user has no selected command
	Then user cannot execute remove

Scenario: User can execute remove with a command selected
	Given user is editing a command set
	Given user has selected a command
	Then user can execute remove