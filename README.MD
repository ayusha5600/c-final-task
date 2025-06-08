# Aayusha Kattel – File Scanner Project (Distributed Systems)
## Project Summary
In this project, I made 3 separate C# console applications that work together to scan text files and count words

## What Each Part Does

### ScannerA
- Reads `.txt` files from a folder
- Counts how many times each word appears
- Sends that data to the Master using a pipe called `agent1`

### ScannerB
- Same as ScannerA but works on another folder.
- Sends data to Master through a different pipe `agent2`

### Master
- Waits for both Scanners to send their data
- Takes the word counts from both, combines them
- Shows the final word count results on the screen

## Example Output

Data received from agent1.  
Data received from agent2.  

--- Merged Word Count Results ---  
scannerB.TXT:my:1  
scannerB.TXT:surname:1  
scannerB.TXT:is:1  
scannerB.TXT:kattel:1  
scannerB.TXT:and:1  
scannerB.TXT:am:1  
scannerB.TXT:very:1  
scannerB.TXT:happy:1  
scannerB.TXT:to:1  
scannerB.TXT:see:1  
scannerB.TXT:you:1  
scannerB.TXT:all:1  
scannerA.txt:my:1  
scannerA.txt:name:1  
scannerA.txt:is:1  
scannerA.txt:aayusha:1  
scannerA.txt:i:1  
scannerA.txt:am:1  
scannerA.txt:currently:1  
scannerA.txt:studying:1  
scannerA.txt:in:1  
scannerA.txt:vilnius:1  
scannerA.txt:university:1

## How I Built and Ran It

1. I used notepad  to write all code  (ScannerA, ScannerB, Master)  
2. I opened 3 different seperate  terminals:  
   - First, I ran the ScannerA  
   - Then I ran ScannerB  
   - Finally, I ran master

3. I passed the folder paths and pipe names as arguments like this:  
cd "C:\Users\Administrator\Documents\oop\scanner project\master\bin\Debug\net6.0"  
master.exe agent1 agent2

cd "C:\Users\Administrator\Documents\oop\scanner project\scannerA\bin\Debug\net6.0"  
scannerA.exe "C:\Users\Administrator\Documents\oop\scanner project\scannerA" agent1 

cd "C:\Users\Administrator\Documents\oop\scanner project\scannerB\bin\Debug\net6.0"  
scannerB.exe "C:\Users\Administrator\Documents\oop\scanner project\scannerB" agent2

## Files in My Project

- ScannerA/Program.cs / ScannerA.csproj  
- ScannerB/Program.cs / ScannerB.csproj  
- Master/Program.cs/master.csproj  
- WordCount.cs  
- scannerA.txt  
- scannerB.txt  
- testing report.pdf
