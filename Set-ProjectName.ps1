param ($libraryName = { throw "-libraryName required" } )

dir -rec . *NerdBank.StyleCop.Rules* |% { ren $_.FullName $_.Name.Replace("NerdBank.StyleCop.Rules", $libraryName) -whatif }

