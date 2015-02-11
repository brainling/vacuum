@storage
Feature: StorageService
	In order to function correctly, we must safely store and retrieve the users data

Scenario: inserting a new document
	Given storage service
	Given document 'Test' in collection 'Foo'
	When document is stored
	Then document 'Test' in collection 'Foo' exists

Scenario: document storage is case insensitive
	Given storage service
	Given document 'Test' in collection 'Foo'
	When document is stored
	Then document 'TEST' in collection 'FOO' exists
