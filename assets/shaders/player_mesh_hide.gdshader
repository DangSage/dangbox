shader_type spatial;

uniform bool hide_head_for_fp = false;
uniform int head_bone_index : hint_range(0, 64) = 0;
uniform int left_arm_bone_index : hint_range(0, 64) = 1;
uniform int right_arm_bone_index : hint_range(0, 64) = 2;

void vertex() {
    // If this is first-person view and vertex belongs to head or arm bones
    if (hide_head_for_fp) {
        // Check if any of the bone influences include the head or arm bones
        bool hide_vertex = false;
        int indices[4] = int[4](int(BONE_INDICES.x), int(BONE_INDICES.y), int(BONE_INDICES.z), int(BONE_INDICES.w));
        for (int i = 0; i < 4; i++) {
            if (indices[i] == head_bone_index || 
                indices[i] == left_arm_bone_index ||
                indices[i] == right_arm_bone_index) {
                hide_vertex = true;
                break;
            }
        }
        if (hide_vertex) {
            VERTEX *= 0.0;
        }
    }
}

void fragment() {
    ALBEDO = vec3(1.0, 1.0, 1.0);
}
