// See https://aka.ms/new-console-template for more information

using Blitz.Compiler;

const string source =
"""
import std;
import std::collections;

let a = 10; !! Type inference
var b: f32 = 15.5; !! Mutable global variable
comp c: i8 = 255; !! Compile time variable

fn main()
{
    ret 0;
}

fn add(x i32, y i32) i32
{
    ret x + y;
}
""";

var tokenizer = new Tokenizer();
var tokens = tokenizer.Tokenize(source);

foreach (var token in tokens)
{
    Console.WriteLine(token);
}