"""Builds TrackHubSans-{Regular,Bold}.ttf: a Latin-script subset of Liberation Sans, renamed as
the OFL requires for a Modified Version. SkiaSharp embeds typefaces whole, so the subset is what
keeps a rendered PDF small.

    pip install fonttools
    python build-fonts.py LiberationSans-Regular.ttf LiberationSans-Bold.ttf
"""
import sys
from fontTools import subset
from fontTools.ttLib import TTFont

UNICODES = "U+0000-024F,U+02B0-02FF,U+2000-206F,U+20A0-20CF,U+2100-214F,U+2190-2199,U+2200-22FF,U+25A0-25FF,U+2713-2718,U+FB01-FB02,U+FFFD"
FAMILY = "TrackHub Sans"


def build(source: str, style: str) -> None:
    font = TTFont(source)
    options = subset.Options(layout_features=[], hinting=False, name_IDs=["*"], notdef_outline=True)
    subsetter = subset.Subsetter(options)
    subsetter.populate(unicodes=subset.parse_unicodes(UNICODES))
    subsetter.subset(font)

    postscript = f"TrackHubSans-{style}"
    full = FAMILY if style == "Regular" else f"{FAMILY} {style}"
    for record in font["name"].names:
        value = {1: FAMILY, 3: f"{FAMILY} {style}; subset of Liberation Sans 2.1.5", 4: full, 6: postscript, 16: FAMILY}.get(record.nameID)
        if value is not None:
            record.string = value

    font.save(f"{postscript}.ttf")
    print(postscript, len(font.getGlyphOrder()), "glyphs")


if __name__ == "__main__":
    build(sys.argv[1], "Regular")
    build(sys.argv[2], "Bold")
