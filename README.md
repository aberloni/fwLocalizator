https://github.com/Legogo/fwLocalizator

# features

- extract data from a spreadsheet (using url & tab uid)
- store it locally in CSV format
- parse all field and make a binary of it
- generate a final txt file with just a pile of key=value

# parse rules

- ignoring empty lines
	an "empty line" is a line with less than 2 fields with content

- auto fill UIDs for lines that are next to each other
	an "empty uid" is a field with less than 3 characters

- UID index starts at 01 (not 00)
	format :  [UID]-{00}