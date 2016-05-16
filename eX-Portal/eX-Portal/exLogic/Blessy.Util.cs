﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public partial class Util {
    public static string RandomPassword() {
      string paswd = System.Web.Security.Membership.GeneratePassword(7, 1).ToString();
      return paswd;
    }

    public static String getNewPassword() {
      var Animals = new String[] {
      "Alligator",   "camel",         "Crow",        "Giraffe",
      "Alpaca",      "Carabao",       "Deer",        "Gnat",
      "Ant",         "Caribou",       "Dinosaur",    "Gnu",
      "Antelope",    "Cat",           "Dog",         "Goat",
      "Ape",         "Cattle",        "Dolphin",     "Goldfish",
      "Armadillo",   "Cheetah",       "Duck",        "Gorilla",
      "Baboon",      "Chimpanzee",    "Eel",         "Grasshopper",
      "Badger",      "Chinchilla",    "Elephant",    "Guinea",
      "Bat",         "Cicada",        "Elk",         "Hamster",
      "Bear",        "Clam",          "Ferret",      "Hare",
      "Beaver",      "Cockroach",     "Fish",        "Hedgehog",
      "Bee",         "Cod",           "Fly",         "Herring",
      "Beetle",      "Coyote",        "Fox",         "Hippopotamus",
      "Buffalo",     "Crab",          "Frog",        "Hornet",
      "Butterfly",   "Cricket",       "Gerbil",      "Horse",
      "Hound",       "Oyster",        "Mouse",       "Minnow",
      "Hyena",       "Panda",         "Mule",        "Mole",
      "Impala",      "Pig",           "Muskrat",     "Monkey",
      "Insect",      "Platypus",      "Otter",       "Moose",
      "Jackal",      "Porcupine",     "Ox",          "Mosquito",
      "Jellyfish",   "Prairiedog",    "Tiger",       "Snail",
      "Kangaroo",    "Pug",           "Trout",       "Snake",
      "Koala",       "Rabbit",        "Turtle",      "Spider",
      "Leopard",     "Raccoon",       "Walrus",      "Squirrel",
      "Lion",        "Reindeer",      "Wasp",        "Termite",
      "Lizard",      "Rhinoceros",    "Weasel",
      "Llama",       "Salmon",        "Whale",
      "Locust",      "Sardine",       "Wolf",
      "Louse",       "Scorpion",      "Wombat",
      "Mallard",     "Seal",          "Woodchuck",
      "Mammoth",     "Serval",        "Worm",
      "Manatee",     "Shark",         "Yak",
      "Marten",      "Sheep",         "Yellowjacket",
      "Mink",        "Skunk",         "Zebra"};
      var RColors = new String[] {
      "AliceBlue",       "DarkBlue",      "FireBrick",     "LightBlue",
      "AntiqueWhite",    "DarkCyan",      "FloralWhite",   "LightCoral",
      "Aqua",            "DarkGoldenRod", "ForestGreen",   "LightCyan",
      "Aquamarine",      "DarkGray",      "Fuchsia",       "LightGoldenRodYellow",
      "Azure",           "DarkGreen",     "Gainsboro",     "LightGray",
      "Beige",           "DarkKhaki",     "GhostWhite",    "LightGreen",
      "Bisque",          "DarkMagenta",   "Gold",          "LightPink",
      "Black",           "DarkOliveGreen","GoldenRod",     "LightSalmon",
      "BlanchedAlmond",  "DarkOrange",    "Gray",          "LightSeaGreen",
      "Blue",            "DarkOrchid",    "Green",         "LightSkyBlue",
      "BlueViolet",      "DarkRed",       "GreenYellow",   "LightSlateGray",
      "Brown",           "DarkSalmon",    "HoneyDew",      "LightSteelBlue",
      "BurlyWood",       "DarkSeaGreen",  "HotPink",       "LightYellow",
      "CadetBlue",       "DarkSlateBlue", "IndianRed",     "Lime",
      "Chartreuse",      "DarkSlateGray", "Indigo",        "LimeGreen",
      "Chocolate",       "DarkTurquoise", "Ivory",         "Linen",
      "Coral",           "DarkViolet",    "Khaki",         "Magenta",
      "CornflowerBlue",  "DeepPink",      "Lavender",      "Maroon",
      "Cornsilk",        "DeepSkyBlue",   "LavenderBlush", "MediumAquaMarine",
      "Crimson",         "DimGray",       "LawnGreen",     "MediumBlue",
      "Cyan",            "DodgerBlue",    "LemonChiffon",  "MediumOrchid",
      "MediumPurple",       "OldLace",        "Peru",        "SeaShell",
      "MediumSeaGreen",     "Olive",          "Pink",        "Sienna",
      "MediumSlateBlue",    "OliveDrab",      "Plum",        "Silver",
      "MediumSpringGreen",  "Orange",         "PowderBlue",  "SkyBlue",
      "MediumTurquoise",    "OrangeRed",      "Purple",      "SlateBlue",
      "MediumVioletRed",    "Orchid",         "Red",         "SlateGray",
      "MidnightBlue",       "PaleGoldenRod",  "RosyBrown",   "Snow",
      "MintCream",          "PaleGreen",      "RoyalBlue",   "SpringGreen",
      "MistyRose",          "PaleTurquoise",  "SaddleBrown", "SteelBlue",
      "Moccasin",           "PaleVioletRed",  "Salmon",      "Tan",
      "NavajoWhite",        "PapayaWhip",     "SandyBrown",  "Teal",
      "Navy",               "PeachPuff",      "SeaGreen",    "Thistle",
      "Tomato",         "Violet", "White",      "Yellow",
      "Turquoise",      "Wheat",  "WhiteSmoke", "YellowGreen"
      };

      var Symbols = "!@#$%^&*|".Split();
      var R = new Random();
      var Password = RColors[R.Next(RColors.Length)] +
         Animals[R.Next(Animals.Length)] +
         Symbols[R.Next(Symbols.Length)] +
         R.Next(100, 999);
      return Password;
    }
  }
}