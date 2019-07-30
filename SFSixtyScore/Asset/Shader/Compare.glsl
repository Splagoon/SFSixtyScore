#version 130

uniform vec4 expectedColors[2];

uniform sampler2D inputTexture;
uniform sampler2D maskTexture;

out vec4 outColor;

void main() {
  vec2 maskCoord = gl_FragCoord.xy / vec2(13.0, 15.0);
  vec4 mask = texture(maskTexture, vec2(maskCoord.x, 1.0 - maskCoord.y));

  vec4 input = texture(inputTexture, gl_TexCoord[0].xy);
  float inclusiveDiff = 1.0;
  for (int i = 0; i < expectedColors.length(); ++i) {
    inclusiveDiff = min(inclusiveDiff, distance(input, expectedColors[i]));
  }

  float exclusiveDiff = 1.0 - (inclusiveDiff * (1.0 - mask.r));
  inclusiveDiff = inclusiveDiff * mask.r;
  float totalDiff = min(inclusiveDiff, exclusiveDiff);
  
  outColor = vec4(totalDiff, 0.0, 0.0, 1.0);
}
