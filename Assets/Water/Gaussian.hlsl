// x: distance from x0
float gaussian1(float x, float sigma) {
    return exp(-(x * x) / (2.0 * sigma * sigma));
}

// x: distance from x0
// y: distance from y0
float gaussian2(float x, float y, float sigma) {
    return exp(-(x * x + y * y) / (2.0 * sigma * sigma));
}

