public class PorterStemmer
{
	private char[] wordArray;   // character array copy of the given string
	private int stem, end;      // indices to the current end (last letter) of the stem and the word in the array

	// Get the stem of a word at least three letters long:
	public string StemWord(string word)
	{
		if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
			return word;

		wordArray = word.ToCharArray();
		stem = 0;
		end = word.Length - 1;

		Step1();
		Step2();
		Step3();
		Step4();
		Step5();
		Step6();

		return new string(wordArray, 0, end + 1);
	}

	// Step 1: remove basic plurals and -ed/-ing:
	private void Step1()
	{
		if (wordArray[end] == 's')
		{
			if (EndsWith("sses"))
				Truncate(2);
			else if (EndsWith("ies"))
				OverwriteEnding("i");
			else if (wordArray[end - 1] != 's')
				Truncate();
		}

		if (EndsWith("eed"))
		{
			if (ConsonantSequenceCount() > 0)
				Truncate();
		}
		else if ((EndsWith("ed") || EndsWith("ing")) && VowelInStem())
		{
			end = stem;
			if (EndsWith("at"))
				OverwriteEnding("ate");
			else if (EndsWith("bl"))
				OverwriteEnding("ble");
			else if (EndsWith("iz"))
				OverwriteEnding("ize");
			else if (EndsWithDoubleConsonant())
			{
				if (!"lsz".Contains(wordArray[end - 1]))
					Truncate();
			}
			else if (ConsonantSequenceCount() == 1 && PrecededByCVC(end))
				OverwriteEnding("e");
		}
	}

	// Step 2: change a terminal 'y' to 'i' if there is another vowel in the stem:
	private void Step2()
	{
		if (EndsWith("y") && VowelInStem())
			OverwriteEnding("i");
	}

	// Step 3: fold double suffixes to single suffix, e.g., -ization = -ize + -ation -> -ize:
	private void Step3()
	{
		switch (wordArray[end - 1])
		{
			case 'a':
				if (ReplaceEnding("ational", "ate")) break;
				ReplaceEnding("tional", "tion"); break;
			case 'c':
				if (ReplaceEnding("enci", "ence")) break;
				ReplaceEnding("anci", "ance"); break;
			case 'e':
				ReplaceEnding("izer", "ize"); break;
			case 'l':
				if (ReplaceEnding("bli", "ble")) break;
				if (ReplaceEnding("alli", "al")) break;
				if (ReplaceEnding("entli", "ent")) break;
				if (ReplaceEnding("eli", "e")) break;
				ReplaceEnding("ousli", "ous"); break;
			case 'o':
				if (ReplaceEnding("ization", "ize")) break;
				if (ReplaceEnding("ation", "ate")) break;
				ReplaceEnding("ator", "ate"); break;
			case 's':
				if (ReplaceEnding("alism", "al")) break;
				if (ReplaceEnding("iveness", "ive")) break;
				if (ReplaceEnding("fulness", "ful")) break;
				ReplaceEnding("ousness", "ous"); break;
			case 't':
				if (ReplaceEnding("aliti", "al")) break;
				if (ReplaceEnding("iviti", "ive")) break;
				ReplaceEnding("biliti", "ble"); break;
			case 'g':
				ReplaceEnding("logi", "log"); break;
		}
	}

	// Step 4: replace -ic-, -full, -ness, etc. with simpler endings:
	private void Step4()
	{
		switch (wordArray[end])
		{
			case 'e':
				if (ReplaceEnding("icate", "ic")) break;
				if (ReplaceEnding("ative", "")) break;
				ReplaceEnding("alize", "al"); break;
			case 'i':
				ReplaceEnding("iciti", "ic"); break;
			case 'l':
				if (ReplaceEnding("ical", "ic")) break;
				ReplaceEnding("ful", ""); break;
			case 's':
				ReplaceEnding("ness", ""); break;
		}
	}

	// Step 5: remove -ant, -ence, etc.:
	private void Step5()
	{
		switch (wordArray[end - 1])
		{
			case 'a':
				if (EndsWith("al")) break; return;
			case 'c':
				if (EndsWith("ance")) break;
				if (EndsWith("ence")) break; return;
			case 'e':
				if (EndsWith("er")) break; return;
			case 'i':
				if (EndsWith("ic")) break; return;
			case 'l':
				if (EndsWith("able")) break;
				if (EndsWith("ible")) break; return;
			case 'n':
				if (EndsWith("ant")) break;
				if (EndsWith("ement")) break;
				if (EndsWith("ment")) break;
				if (EndsWith("ent")) break; return;
			case 'o':
				if (EndsWith("ion") && stem >= 0 && (wordArray[stem] == 's' || wordArray[stem] == 't')) break;
				if (EndsWith("ou")) break; return;
			case 's':
				if (EndsWith("ism")) break; return;
			case 't':
				if (EndsWith("ate")) break;
				if (EndsWith("iti")) break; return;
			case 'u':
				if (EndsWith("ous")) break; return;
			case 'v':
				if (EndsWith("ive")) break; return;
			case 'z':
				if (EndsWith("ize")) break; return;
			default:
				return;
		}

		if (ConsonantSequenceCount() > 1)
			end = stem;
	}

	// Step 6: remove final 'e' if necessary:
	private void Step6()
	{
		stem = end;
		if (wordArray[end] == 'e')
		{
			var m = ConsonantSequenceCount();
			if (m > 1 || m == 1 && !PrecededByCVC(end - 1))
				Truncate();
		}

		if (wordArray[end] == 'l' && EndsWithDoubleConsonant() && ConsonantSequenceCount() > 1)
			Truncate();
	}

	private void Truncate(int n = 1)
	{
		end -= n;
	}

	// Count the number of CVC sequences:
	private int ConsonantSequenceCount()
	{
		int m = 0, index = 0;
		for (; index <= stem && IsConsonant(index); index++) ;
		if (index > stem)
			return 0;

		for (index++; ; index++)
		{
			for (; index <= stem && !IsConsonant(index); index++) ;
			if (index > stem)
				return m;

			for (index++, m++; index <= stem && IsConsonant(index); index++) ;
			if (index > stem)
				return m;
		}
	}

	// Return true if there is a vowel in the current stem:
	private bool VowelInStem()
	{
		for (var i = 0; i <= stem; i++)
			if (!IsConsonant(i))
				return true;
		return false;
	}

	// Returns true if the character at the specified index is a consonant, with special handling for 'y':
	private bool IsConsonant(int index)
	{
		if ("aeiou".Contains(wordArray[index]))
			return false;

		return wordArray[index] != 'y' || index == 0 || !IsConsonant(index - 1);
	}

	// Return true if the char. at the current index and the one preceeding it are the same consonant:
	private bool EndsWithDoubleConsonant()
	{
		return end > 0 && wordArray[end] == wordArray[end - 1] && IsConsonant(end);
	}

	// Check if the letters at i-2, i-1, i have the pattern: consonant-vowel-consonant (CVC) and the second consonant
	// is not w, x or y; used when restoring an 'e' at the end of a short word, e.g., cav(e), lov(e), hop(e), etc.:
	private bool PrecededByCVC(int index)
	{
		if (index < 2 || !IsConsonant(index) || IsConsonant(index - 1) || !IsConsonant(index - 2))
			return false;

		return !"wxy".Contains(wordArray[index]);
	}

	// Check if the given string appears at the end of the word:
	private bool EndsWith(string s)
	{
		int length = s.Length, index = end - length + 1;
		if (index >= 0)
		{
			for (var i = 0; i < length; i++)
				if (wordArray[index + i] != s[i])
					return false;

			stem = end - length;
			return true;
		}
		return false;
	}

	// Conditionally replace the end of the word:
	private bool ReplaceEnding(string suffix, string s)
	{
		if (EndsWith(suffix) && ConsonantSequenceCount() > 0)
		{
			OverwriteEnding(s);
			return true;
		}
		return false;
	}

	// Change the end of the word to a given string:
	private void OverwriteEnding(string s)
	{
		int length = s.Length, index = stem + 1;
		for (var i = 0; i < length; i++)
			wordArray[index + i] = s[i];
		end = stem + length;
	}
}
