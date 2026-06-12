const { Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
        AlignmentType, HeadingLevel, WidthType, BorderStyle, ShadingType,
        LevelFormat } = require("docx");
const fs = require("fs");

const border = { style: BorderStyle.SINGLE, size: 1, color: "CCCCCC" };
const borders = { top: border, bottom: border, left: border, right: border };

const rtlPara = (text, opts = {}) => new Paragraph({
    bidirectional: true,
    alignment: AlignmentType.RIGHT,
    ...opts,
    children: [new TextRun({ text, font: "Arial", size: 24, ...opts.run })]
});

const doc = new Document({
    numbering: {
        config: [{
            reference: "bullets",
            levels: [{ level: 0, format: LevelFormat.BULLET, text: "•",
                alignment: AlignmentType.RIGHT,
                style: { paragraph: { indent: { left: 720, hanging: 360 },
                    bidirectional: true } } }]
        }]
    },
    sections: [{
        children: [
            // Title
            new Paragraph({
                bidirectional: true,
                alignment: AlignmentType.RIGHT,
                children: [new TextRun({
                    text: "מדריך הגנה — פרויקט Checkers ברשת",
                    font: "Arial", size: 36, bold: true
                })]
            }),
            new Paragraph({ children: [] }),

            // Paragraph
            rtlPara("בנינו משחק דמקה ברשת. הפרויקט מורכב מ-4 חלקים שעובדים יחד."),
            new Paragraph({ children: [] }),

            // Table
            new Table({
                width: { size: 9000, type: WidthType.DXA },
                columnWidths: [1500, 3000, 4500],
                bidiVisual: true,
                rows: [
                    new TableRow({ children: [
                        new TableCell({ borders, width: { size: 1500, type: WidthType.DXA },
                            shading: { fill: "2E75B6", type: ShadingType.CLEAR },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "מספר", font: "Arial", size: 22, bold: true, color: "FFFFFF" })] })] }),
                        new TableCell({ borders, width: { size: 3000, type: WidthType.DXA },
                            shading: { fill: "2E75B6", type: ShadingType.CLEAR },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "שם", font: "Arial", size: 22, bold: true, color: "FFFFFF" })] })] }),
                        new TableCell({ borders, width: { size: 4500, type: WidthType.DXA },
                            shading: { fill: "2E75B6", type: ShadingType.CLEAR },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "מה הוא עושה", font: "Arial", size: 22, bold: true, color: "FFFFFF" })] })] }),
                    ]}),
                    new TableRow({ children: [
                        new TableCell({ borders, width: { size: 1500, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "1", font: "Arial", size: 22 })] })] }),
                        new TableCell({ borders, width: { size: 3000, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "CheckersGame.Client", font: "Arial", size: 22 })] })] }),
                        new TableCell({ borders, width: { size: 4500, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "תוכנת Windows שמציגה את לוח המשחק", font: "Arial", size: 22 })] })] }),
                    ]}),
                    new TableRow({ children: [
                        new TableCell({ borders, width: { size: 1500, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "2", font: "Arial", size: 22 })] })] }),
                        new TableCell({ borders, width: { size: 3000, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "CheckersGame.Api", font: "Arial", size: 22 })] })] }),
                        new TableCell({ borders, width: { size: 4500, type: WidthType.DXA },
                            children: [new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                                children: [new TextRun({ text: "שרת שמקבל צעדים ומחזיר תשובה", font: "Arial", size: 22 })] })] }),
                    ]}),
                ]
            }),
            new Paragraph({ children: [] }),

            // Bullet list
            new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                numbering: { reference: "bullets", level: 0 },
                children: [new TextRun({ text: "ה-Client שולח צעד ל-Api", font: "Arial", size: 24 })] }),
            new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                numbering: { reference: "bullets", level: 0 },
                children: [new TextRun({ text: "ה-Api בודק שהצעד חוקי", font: "Arial", size: 24 })] }),
            new Paragraph({ bidirectional: true, alignment: AlignmentType.RIGHT,
                numbering: { reference: "bullets", level: 0 },
                children: [new TextRun({ text: "ה-Api מחזיר את הצעד של ה-Server", font: "Arial", size: 24 })] }),
        ]
    }]
});

Packer.toBuffer(doc).then(buf => {
    fs.writeFileSync("C:\\Visual Studio\\Final project\\test_hebrew.docx", buf);
    console.log("Done!");
});
