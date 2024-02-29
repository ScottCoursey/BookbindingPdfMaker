# BookbindingPdfMaker
A program to convert a single PDF into signatures for bookbinding.  There is an "Output Test Overlay" option to make sure the output will fit on your pages.  The primary feature I wanted to add to this program is to "stack" a page for output so it can be run through a paper cutter to get the grain in the proper direction more easily than purchasing specialty paper.  By "stack", I mean a single face of an output page contains four pages from the PDF.  Thinking of legal-size paper, the top half of a cut would result in a signature that is 8.5" x 7", and folds to be a small notebook that is 4.25" x 7".  The bottom half of the cut would be the next signature.

# Installation
The installation is pretty straightforward.
1. Visit the <a href="https://github.com/ScottCoursey/BookbindingPdfMaker/releases">releases area</a>.
2. Choose the version you want to install (usually the most current).
3. Locate the Assets and click the installer (.msi file).
4. Run the file on your hard drive and go through the installer.

If you have a previous version already installed, you'll be prompted if you want to remove the previous one.

# Overview

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/105a6865-cc56-4751-9eb3-460ee55b63ed)

The program is broken into several blocks so you may customize your output.

## Input PDF Info

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/f69a5659-3d26-46f7-a9df-de18a016152c)

This panel lets you choose your input PDF file and the program's output folder by clicking the buttons on the right of each line and the options are also available in the menu bar at the top.  If you select an output folder which already has content, it will prompt you whether you want the folder and its contents removed and will do so if you confirm.

## Units

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/eaba86e9-31c2-45dc-a4af-3d7aa24172fb)

The units are a convenience so you may enter values based on your comfort level.  It's mostly used for the custom width and height of the book size.

## Printer

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/d62720cb-f236-41cf-94ea-d4b9262da6ce)

Here you can set various options for the physical output.  The paper size dropdown has many types of paper and if you need something added or updated, please let me know and I'll add it to the list.  The printer type lets you determine if your pages are printed as double sided or single sheets.  Currently, the single sheets will just spew the pages but I need to update it so that there will be a pause for allowing the user to flip the pages and put them back into the printer.  I use duplex, so I know it works the best.  =)

The Do Not Stack Layout will consume an entire left and right page to be a full set of PDF input pages.  That is, you can print, fold, and bind.  The entire output is a signature.  Alternating the page rotation further assists the duplex printing by rotating every other page so that when the signatures are folded, everything lines up properly.

The Layout Is Stacked will require you to cut the paper horizontally because the top and bottom halves are separate, consecutive signatures.  This was added to assist in having the grain be the correct direction for smaller projects, like a 4.25" x 7" book made from legal paper.

## Book Size

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/3004c006-97a2-4ec4-8a9f-3140e713c9d3)

The book size is different than the paper size because it's the printed area on each of the pages where a Standard Paperback will be 4.75" x 7" and a Large Paperback is 6" x 8".  For the Full Paper Size, the program will compute the scaling based on the size of paper it will be targeting.  Custom is custom.  You tell it the size of the book and it will scale the page accordingly.  Keep Proportion by with or height will find the aspect ratio of the input document's page and scale the book based on the edge you want.

## Options

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/d28b1b39-372f-4cd3-8ff9-edd78f136406)

Add Flyleaf will add an extra page and the beginning and end of the text in case you need them for binding.

I recommend using the Output Test Overlay so that you will get output guides in the resulting PDF and check your work.  It will put a light blue border around the book size and pink line(s) for the page spine and in the case of stacking the layout, it will add a red line for the cut mark.

How does it scale?  That's with Page Scaling, of course.  If you choose Stretch To Fit, the output will match the section to which it is printing.  This might be ideal, but scaling some text might result in some wonky results.  

Here is an example of a single printed page with a stacked layout, custom book size of 4" x 5", printing on Letter paper, and the overlay turned on:

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/835dc57d-a424-4a05-b656-05d6a391b0ea)

As the program calculates how many pages are in a signature, there may be times when a signature set at the top and bottom have different sizes.  If that is the case, the resulting section of the output will have an "X" printed across it, to let you know that portion of the page should be discarded.  For example:

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/0ea6debc-fc73-4efd-8f9e-452a536877a0)

## Signature Format

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/a0eaf175-238c-4c61-82d2-82f790eea94b)

This allows you to choose the style of signatures you want output.  For Booklet, it will use 4 printed pages per signature.  Perfect Bound is a single printed page per signature.  Standard Signatures is 8 printed pages per signature.  Custom Signatures accepts a list of signature sizes of your choosing, separated by a space.  If you have a need for three four-page signatures, three eights, then two three-page signatures, you would enter "4 4 4 8 8 8 3 3".

## Signature Info

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/dcd3721c-8f6c-4cc0-a7d6-1c7e124b0bf5)

This summarizes the output for you.  Total Pages is how many sheets, plus the optional flyway, will be considered for input.  The Total Sheets is how many pages will actually be printed.  And the Number of Signatures indicates how many signature sets will be generated.

## Generate Document

![image](https://github.com/ScottCoursey/BookbindingPdfMaker/assets/48330690/b6ee841f-0b23-4941-9466-bca6c64380a3)

This wonderful button will only be enabled once you have an input PDF and an output folder selected.

# Suggestions?  Tips?
Please feel free to contact me if you can think of something to add or if something is broken.  Or, you can use the Issues tab to file a ticket so I can track it.

# Attributions
This is by no means an original idea.  I used <a href="http://www.quantumelephant.co.uk/bookbinder/bookbinder.html">Bookbinder</a> as inspiration but I did not have access to the source code (nor would I dream of decompiling it), so I had to make my own.  I have lots of respect for this program, so a huge thanks to the original author.

I'm too lazy to draw my own icon right now, so I used one from Flaticon.  The icon comes from: <a href="https://www.flaticon.com/free-icons/book" title="book icons">Book icons created by Freepik - Flaticon</a>
