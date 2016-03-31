using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public static class Weather {
    public static Dictionary<int, String> WeatherIcons = new Dictionary<int, String>() {
     {0, "&#xf056;" }, {10, "&#xf019;" },  {20, "&#xf014;" }, {30, "&#xf00c;" },   {40, "&#xf01a;" },
     {1, "&#xf01d;" }, {11, "&#xf01a;" },  {21, "&#xf0b6;" }, {31, "&#xf02e;" },   {41, "&#xf01b;" },
     {2, "&#xf073;" }, {12, "&#xf01a;" },  {22, "&#xf062;" }, {32, "&#xf00d;" },   {42, "&#xf01b;" },
     {3, "&#xf01e;" }, {13, "&#xf01b;" },  {23, "&#xf085;" }, {33, "&#xf02e;" },   {43, "&#xf01b;" },
     {4, "&#xf01e;" }, {14, "&#xf01b;" },  {24, "&#xf021;" }, {34, "&#xf00d;" },   {44, "&#xf013;" },
     {5, "&#xf017;" }, {15, "&#xf064;" },  {25, "&#xf076;" }, {35, "&#xf015;" },   {45, "&#xf01e;" },
     {6, "&#xf0b5;" }, {16, "&#xf01b;" },  {26, "&#xf013;" }, {36, "&#xf072;" },   {46, "&#xf01b;" },
     {7, "&#xf0b5;" }, {17, "&#xf015;" },  {27, "&#xf086;" }, {37, "&#xf01e;" },   {47, "&#xf01d;" },
     {8, "&#xf04e;" }, {18, "&#xf0b5;" },  {28, "&#xf002;" }, {38, "&#xf01e;" },
     {9, "&#xf04e;" }, {19, "&#xf063;" },  {29, "&#xf083;" }, {39, "&#xf01e;" },
     
     {3200, "&#xf075;" }     
    };
  }
}