var memory = new Object();
var acc = "";
var ix = "";

var term = new Object();

var accDisplay = document.getElementById("acc");
console.log(accDisplay)
var ixDisplay = document.getElementById("ix");

var table = document.getElementById("memory");

function Initialize(terminal)
{
    term = terminal;
    WriteLine("YAssembler Initialized, Version 0.1 by Jingcheng Yang")
}

function WriteLine(line)
{
    term.write(">> " + line + '\r\n')
}

function AssembleLine(line)
{
    ParseLine(line);
    accDisplay.innerText = acc;
    ixDisplay.innerText = ix;

    var items = Object.keys(memory).map(function(key) {
        return [key, memory[key]];
    });

    items.sort(function(first, second) {
        return second[1] - first[1];
    });

    console.log(items)

    for(var i = 1; i < table.rows.length;)
    {   
        table.deleteRow(i);
    }

    items.forEach(element => {
        console.log(element[0])
        var row = table.insertRow();
        var cell1 = row.insertCell(0);
        var cell2 = row.insertCell(1);
        cell1.innerHTML = element[0]
        cell2.innerHTML = element[1]
    });

    
}

function ParseLine(line)
{
    console.log(line)

    var instructions = line.split(" ");
    if(instructions.length == 0)
    {
        return;
    }

    if(instructions.length == 1)
    {
        var opcode = instructions[0]
        if(opcode == "MOV")
        {
            ix = acc;
        }
    }

    if(instructions.length == 2)
    {
        var opcode = instructions[0]
        var oprand = instructions[1]
        if(opcode == "LDM")
        {
            acc = oprand.substring(1);
        }
        if(opcode == "LDR")
        {
            ix = oprand.substring(1);
        }
        if(opcode == "LDD")
        {
            acc = memory[oprand]
        }
        if(opcode == "LDI")
        {
            acc = memory[memory[oprand]]
        }
        if(opcode == "LDX")
        {
            var address = Number(oprand) + Number(ix);
            acc = memory[address];
        }
        if(opcode == "STO")
        {
            memory[oprand] = acc;
        }
    }
}