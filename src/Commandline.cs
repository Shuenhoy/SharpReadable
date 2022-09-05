using CommandLine;

namespace SharpReadable
{
    class Options
    {
        [Option('s', "skip", HelpText = "skip", Default = 0)]
        public int Skip { get; set; }

        [Option('l', "last", HelpText = "skip last", Default = 0)]
        public int SkipLast { get; set; }

        [Option('r', "refine", HelpText = "refine", Default = true)]
        public bool Refine { get; set; }

        [Option('f', "fill", HelpText = "fill", Default = true)]
        public bool Fill { get; set; }

        [Option('c', "context", HelpText = "context size", Default = 40)]
        public int ContextSize { get; set; }

        [Option('m', "mask", HelpText = "mask", Default = "heuristic")]
        public string Mask { get; set; }

        [Option("apiurl",
          Default = "http://localhost:5000",
          HelpText = "URL of NLP api server")]
        public string APIUrl { get; set; }

        [Value(0, MetaName = "input", HelpText = "input file", Required = true)]
        public string Input { get; set; }

        [Value(1, MetaName = "output", HelpText = "output file", Required = true)]
        public string Output { get; set; }
    }
}