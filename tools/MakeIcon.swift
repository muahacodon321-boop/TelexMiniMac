import AppKit
import Foundation

@main enum MakeIcon {
    static func main() {
        guard CommandLine.arguments.count>1 else { exit(2) }
        let target=CommandLine.arguments[1]
        let size=1024
        guard let rep=NSBitmapImageRep(bitmapDataPlanes:nil,pixelsWide:size,pixelsHigh:size,
                                      bitsPerSample:8,samplesPerPixel:4,hasAlpha:true,
                                      isPlanar:false,colorSpaceName:.deviceRGB,bytesPerRow:0,bitsPerPixel:0),
              let context=NSGraphicsContext(bitmapImageRep:rep) else {exit(3)}
        NSGraphicsContext.saveGraphicsState()
        NSGraphicsContext.current=context
        let bounds=NSRect(x:12,y:12,width:1000,height:1000)
        let bg=NSBezierPath(roundedRect:bounds,xRadius:228,yRadius:228)
        NSGradient(colors:[NSColor(calibratedRed:0.12,green:0.85,blue:0.69,alpha:1),
                           NSColor(calibratedRed:0.08,green:0.49,blue:0.72,alpha:1)])?
            .draw(in:bg,angle:30)
        NSColor(calibratedWhite:1,alpha:0.13).setStroke()
        bg.lineWidth=13;bg.stroke()
        let font=NSFont.systemFont(ofSize:620,weight:.bold)
        let attrs:[NSAttributedString.Key:Any]=[
            .font:font,
            .foregroundColor:NSColor.white,
            .kern:-12.0
        ]
        let label="T" as NSString
        let textSize=label.size(withAttributes:attrs)
        label.draw(at:NSPoint(x:(1024-textSize.width)/2-18,
                              y:(1024-textSize.height)/2-17),withAttributes:attrs)
        NSGraphicsContext.restoreGraphicsState()
        guard let data=rep.representation(using:.png,properties:[:]) else {exit(4)}
        do {try data.write(to:URL(fileURLWithPath:target))} catch {exit(5)}
    }
}
