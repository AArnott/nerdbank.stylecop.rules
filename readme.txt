This is the future home of NerdBank.StyleCop.Rules.

To customize it for a library:
1. Find & Replace in Files with case sensitive search: 
	NerdBank.StyleCop.Rules -> YourLibrary
2. Do a dir /s *NerdBank.StyleCop.Rules* in the root of the project and rename all files/directories to *YourLibrary*.
	 dir -rec . *NerdBank.StyleCop.Rules* |% { ren $_.fullname $_.name.replace("NerdBank.StyleCop.Rules", "YourLibrary") }