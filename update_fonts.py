#!/usr/bin/env python3
import re
import os

def update_font_usage_in_file(file_path):
    """파일에서 TextMeshProUGUI 생성 후 FontManager 적용 추가"""
    
    if not os.path.exists(file_path):
        print(f"파일을 찾을 수 없습니다: {file_path}")
        return
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # TextMeshProUGUI 변수 생성 패턴 찾기
    pattern = r'(\s+)(TextMeshProUGUI\s+\w+)\s*=\s*(\w+)\.AddComponent<TextMeshProUGUI>\(\);'
    
    def replacement(match):
        indent = match.group(1)
        assignment = match.group(2)
        object_name = match.group(3)
        variable_name = assignment.split()[-1]  # TextMeshProUGUI 변수명 추출
        
        return f'{indent}{assignment} = {object_name}.AddComponent<TextMeshProUGUI>();\n{indent}FontManager.ApplyDefaultKoreanFont({variable_name});'
    
    # 패턴 교체 수행
    updated_content = re.sub(pattern, replacement, content)
    
    if updated_content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(updated_content)
        print(f"✅ {file_path} 업데이트 완료")
        
        # 변경된 라인 수 출력
        original_lines = original_content.count('\n')
        updated_lines = updated_content.count('\n')
        print(f"   추가된 라인: {updated_lines - original_lines}개")
    else:
        print(f"⏭️  {file_path} - 변경 사항 없음")

def main():
    # 업데이트할 파일들
    files_to_update = [
        "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/UIPrefabGenerator.cs",
        "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/StudentDetailPopup.cs",
        "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/AddStudentPopup.cs",
        "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/SettingsPanelUI.cs",
        "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/PopupManager.cs"
    ]
    
    print("🔄 폰트 사용 업데이트 시작...")
    print("=" * 50)
    
    for file_path in files_to_update:
        update_font_usage_in_file(file_path)
    
    print("=" * 50)
    print("🎉 모든 파일 업데이트 완료!")

if __name__ == "__main__":
    main()