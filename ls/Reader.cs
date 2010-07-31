using System;
using System.Collections;
using System.Text;
using System.IO;

namespace ls
{
    public interface IMetaReader
    {
        object Read(StringReader inStream);
    }

    public class MetaStringReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume leading quote
            StringBuilder theStringBuilder = new StringBuilder();
            while(true)
            {
                int theChar = inStream.Peek();
                if ((char)theChar == '"' || theChar == -1)
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }
            inStream.Read(); // consume trailing quote

            return theStringBuilder.ToString();
        }
    }

    public class MetaListReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume leading paren
            ArrayList theList = new ArrayList();
            while (true)
            {
                int theChar = inStream.Peek();
                if ((char)theChar == ')' || theChar == -1) // TODO: There are bugs here, what if some wacky '}' is afoot?
                    break;

                theList.Add(Reader.Read(inStream));

                while (char.IsWhiteSpace((char)inStream.Peek()))
                    inStream.Read();
            }
            inStream.Read(); // consume trailing paren

            return theList;
        }
    }

    public class MetaMapReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume leading curly
            Hashtable theMap = new Hashtable();
            while (true)
            {
                int theChar = inStream.Peek();
                if ((char)theChar == '}' || theChar == -1) // TODO: There are bugs here, what if some wacky ')' is afoot?
                    break;

                theMap.Add(Reader.Read(inStream), Reader.Read(inStream));

                while (char.IsWhiteSpace((char)inStream.Peek()))
                    inStream.Read();
            }
            inStream.Read(); // consume trailing curly

            return theMap;
        }
    }

    public class MetaQuoteReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume quote
            ArrayList theList = new ArrayList();
            theList.Add(new Symbol("quote"));
            theList.Add(Reader.Read(inStream));
            return theList;
        }
    }

    public class MetaBackQuoteReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume backtick
            ArrayList theList = new ArrayList();
            theList.Add(new Symbol("backquote"));
            theList.Add(Reader.Read(inStream));
            return theList;
        }
    }

    public class MetaUnQuoteReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume comma
            ArrayList theList = new ArrayList();
            theList.Add(new Symbol("unquote"));
            theList.Add(Reader.Read(inStream));
            return theList;
        }
    }

    public class MetaCommentReader : IMetaReader
    {
        public object Read(StringReader inStream)
        {
            inStream.Read(); // consume semi-colon
            while (true)
            {
                int theChar = inStream.Peek();
                if (theChar == -1 || (char)theChar == '\r' || (char)theChar == '\n')
                    break;

                inStream.Read(); // Discard
            }
            return null;
        }
    }

    public class Reader
    {
        static IMetaReader[] MetaReaders = new IMetaReader[256];
        static Reader()
        {
            MetaReaders['"'] = new MetaStringReader();
            MetaReaders['('] = new MetaListReader();
            MetaReaders['{'] = new MetaMapReader();
            MetaReaders['\''] = new MetaQuoteReader();
            MetaReaders[';'] = new MetaCommentReader();
            MetaReaders['`'] = new MetaBackQuoteReader();
            MetaReaders[','] = new MetaUnQuoteReader();
        }
        
        public static object ReadNumber(StringReader inStream)
        {
            StringBuilder theStringBuilder = new StringBuilder();
            while (true)
            {
                int theChar = inStream.Peek();
                if (char.IsWhiteSpace((char)theChar) || theChar == -1 || (char)theChar == ')' || (char)theChar == '}')
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }

            string theString = theStringBuilder.ToString();
            if (theString.Contains(".")) // TODO: Something much more sophisticated here.
                return double.Parse(theString);
            else
                return int.Parse(theString);

        }

        public static object ReadSymbol(StringReader inStream)
        {
            StringBuilder theStringBuilder = new StringBuilder();
            while (true)
            {
                int theChar = inStream.Peek();
                if (char.IsWhiteSpace((char)theChar) || theChar == -1 || (char)theChar == ')' || (char)theChar == '}')
                    break;

                theStringBuilder.Append((char)inStream.Read());
            }
            return new Symbol(theStringBuilder.ToString());
        }

        public static object Read(string inString)
        {
            return Read(new StringReader(inString));
        }

        public static object Read(StringReader inStream)
        {
            while (char.IsWhiteSpace((char)inStream.Peek()))
                inStream.Read();

            if (-1 == inStream.Peek()) // EOF
                return null;

            char theChar = (char)inStream.Peek();
            if (char.IsDigit(theChar))
                return ReadNumber(inStream);

            if (MetaReaders[theChar]!=null)
                return MetaReaders[theChar].Read(inStream);

            return ReadSymbol(inStream);
        }
    }
}
