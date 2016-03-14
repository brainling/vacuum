@storage
Feature: StorageService
	In order to function correctly, we must safely store and retrieve the users data

Scenario: loading document headers
	Given storage service
	Given document 'Test 1' has been created
	Given document 'Test 2' has been created
	When document headers are loaded
	Then headers contains 'Test 1, Test 2'

Scenario: finding documents by name
	Given storage service
	Given document 'Test' has been created
	Then document 'Test' exists

Scenario: document storage is case insensitive
	Given storage service
	Given document 'Test' has been created
	Then document 'TEST' exists

Scenario: deleting documents by name
	Given storage service
	Given document 'Test' has been created
	When document 'Test' is deleted
	Then document 'Test' does not exist

Scenario: finding documents by id
	Given storage service
	Given document 'Test' has been created
	Then document with id exists

Scenario: deleting documents by id
	Given storage service
	Given document 'Test' has been created
	When document is deleted by id
	Then document with id does not exist
