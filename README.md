# SharpReadble (prototype stage) 

Apply Bionic Reading-like modification on your PDF.

```bash
dotnet run input.pdf output.pdf
```

## Use NLP model
We borrow the python script from Sioyek in `script/server.py`.

To use the model, you need to first run the server.
Then use SharpReadble with
```bash
dotnet run -m http input.pdf output.pdf
```

See https://ahrm.github.io/jekyll/update/2022/04/14/using-languge-models-to-read-faster.html for detail about the NLP model.

## Options
```bash
SharpReadable 1.0.0
Copyright (C) 2022 SharpReadable

  -s, --skip         (Default: 0) skip

  -l, --last         (Default: 0) skip last

  -r, --refine       (Default: true) refine

  -f, --fill         (Default: true) fill

  -c, --context      (Default: 49) context size

  -m, --mask         (Default: heuristic) mask

  --apiurl           (Default: http://localhost:5000) URL of NLP api server

  --help             Display this help screen.

  --version          Display version information.

  input (pos. 0)     Required. input file

  output (pos. 1)    Required. output file
```