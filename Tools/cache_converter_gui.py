"""
DMP Translator - Cache Converter (GUI Version)
번역 캐시 파일 형식 변환 도구

변환 기능:
1. 탭(\\t) 구분 → =(equals) 구분
2. ==>(화살표) 구분 → =(equals) 구분
3. 자동 감지하여 변환
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import os
from pathlib import Path

# tkinterdnd2 import 시도 (선택사항)
try:
    import tkinterdnd2 as tkdnd
    HAS_DND = True
except ImportError:
    HAS_DND = False


class CacheConverterGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("DMP Translator - Cache Converter")
        self.root.geometry("800x600")
        self.root.resizable(True, True)
        
        # 스타일 설정
        self.setup_styles()
        
        # UI 생성
        self.create_widgets()
        
        # 드래그 앤 드롭 활성화 (가능한 경우)
        if HAS_DND:
            self.setup_drag_drop()
        
    def setup_styles(self):
        """스타일 설정"""
        style = ttk.Style()
        style.theme_use('clam')
        
        # 버튼 스타일
        style.configure('Big.TButton', 
                       font=('맑은 고딕', 12, 'bold'),
                       padding=15)
        
    def create_widgets(self):
        """UI 위젯 생성"""
        # 메인 프레임
        main_frame = ttk.Frame(self.root, padding="20")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # 타이틀
        title_label = ttk.Label(main_frame, 
                               text="🔄 DMP Translator Cache Converter",
                               font=('맑은 고딕', 18, 'bold'))
        title_label.grid(row=0, column=0, pady=(0, 10))
        
        # 설명
        desc_label = ttk.Label(main_frame,
                              text="캐시 파일을 = 구분자로 자동 변환\nAuto convert cache file to = separator",
                              font=('맑은 고딕', 10),
                              justify=tk.CENTER)
        desc_label.grid(row=1, column=0, pady=(0, 20))
        
        # 드롭 영역
        self.drop_frame = tk.Frame(main_frame, 
                                   bg='#e8f4f8',
                                   relief=tk.SOLID,
                                   borderwidth=2,
                                   cursor='hand2')
        self.drop_frame.grid(row=2, column=0, pady=20, sticky=(tk.W, tk.E))
        self.drop_frame.configure(height=150)
        
        if HAS_DND:
            drop_text = "📁 translation_cache.txt 파일을 여기에 드롭하세요\nDrop translation_cache.txt here"
        else:
            drop_text = "📁 아래 버튼으로 파일을 선택하세요\nClick button below to select file"
        
        drop_label = tk.Label(self.drop_frame,
                             text=drop_text,
                             font=('맑은 고딕', 14),
                             bg='#e8f4f8',
                             fg='#0066cc')
        drop_label.pack(expand=True)
        
        # 파일 선택 버튼
        select_btn = ttk.Button(main_frame,
                               text="📂 파일 선택 / Select File",
                               style='Big.TButton',
                               command=self.select_file)
        select_btn.grid(row=3, column=0, pady=20)
        
        # 변환 정보
        info_frame = ttk.LabelFrame(main_frame, text="변환 정보 / Conversion Info", padding="10")
        info_frame.grid(row=4, column=0, pady=10, sticky=(tk.W, tk.E))
        
        info_text = tk.Text(info_frame, height=5, font=('맑은 고딕', 9), bg='#f0f0f0', relief=tk.FLAT)
        info_text.pack(fill=tk.BOTH, expand=True)
        info_text.insert('1.0', 
            "✅ 자동 감지 및 변환 / Auto detect and convert:\n"
            "   • 탭(\\t) 구분 → = 구분 / Tab → =\n"
            "   • ==> 구분 → = 구분 / ==> → =\n\n"
            "📝 결과 형식 / Result format:\n"
            "   원문=번역문")
        info_text.config(state='disabled')
        
        # 진행 상황 표시
        self.progress = ttk.Progressbar(main_frame, 
                                       length=700, 
                                       mode='indeterminate')
        self.progress.grid(row=5, column=0, pady=10)
        
        # 로그 영역
        log_frame = ttk.LabelFrame(main_frame, 
                                   text="로그 / Log",
                                   padding="10")
        log_frame.grid(row=6, column=0, pady=10, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # 스크롤바가 있는 텍스트 위젯
        scrollbar = ttk.Scrollbar(log_frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.log_text = tk.Text(log_frame, 
                               height=12,
                               width=90,
                               font=('Consolas', 9),
                               yscrollcommand=scrollbar.set)
        self.log_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.log_text.yview)
        
        # 그리드 가중치 설정
        self.root.columnconfigure(0, weight=1)
        self.root.rowconfigure(0, weight=1)
        main_frame.columnconfigure(0, weight=1)
        main_frame.rowconfigure(6, weight=1)
        
        # 초기 로그
        self.log("=" * 80)
        self.log("🚀 DMP Translator Cache Converter 시작 / Started")
        self.log("=" * 80)
        if HAS_DND:
            self.log("✅ 드래그 앤 드롭 사용 가능 / Drag & Drop available")
        else:
            self.log("⚠️  드래그 앤 드롭 비활성화 (tkinterdnd2 없음) / Drag & Drop disabled")
        self.log("")
        self.log("📌 translation_cache.txt 파일을 선택하세요 / Select translation_cache.txt file")
        
    def setup_drag_drop(self):
        """드래그 앤 드롭 설정"""
        try:
            self.drop_frame.drop_target_register(tkdnd.DND_FILES)
            self.drop_frame.dnd_bind('<<Drop>>', self.on_drop)
        except:
            pass
        
    def on_drop(self, event):
        """드롭 이벤트 핸들러"""
        file_path = event.data
        # tkinterdnd2는 파일 경로를 중괄호로 감싸서 전달할 수 있음
        if file_path.startswith('{') and file_path.endswith('}'):
            file_path = file_path[1:-1]
        
        self.process_file(file_path)
        
    def select_file(self):
        """파일 선택 다이얼로그"""
        file_path = filedialog.askopenfilename(
            title="translation_cache.txt 선택",
            filetypes=[
                ("Text files", "*.txt"),
                ("All files", "*.*")
            ]
        )
        
        if file_path:
            self.process_file(file_path)
    
    def log(self, message):
        """로그 메시지 추가"""
        self.log_text.insert(tk.END, f"{message}\n")
        self.log_text.see(tk.END)
        self.root.update()
        
    def detect_separator(self, lines):
        """구분자 자동 감지"""
        # 샘플로 처음 10줄 확인
        sample_lines = lines[:min(10, len(lines))]
        
        tab_count = sum(1 for line in sample_lines if '\t' in line)
        arrow_count = sum(1 for line in sample_lines if '==>' in line)
        
        if tab_count > arrow_count:
            return 'tab', '\t'
        elif arrow_count > 0:
            return 'arrow', '==>'
        else:
            return 'equal', '='
        
    def process_file(self, file_path):
        """파일 변환 처리"""
        self.log(f"\n{'='*80}")
        self.log(f"파일 선택됨 / File selected: {os.path.basename(file_path)}")
        
        # 파일 존재 확인
        if not os.path.exists(file_path):
            self.log("❌ 파일을 찾을 수 없습니다! / File not found!")
            messagebox.showerror("오류 / Error", "파일을 찾을 수 없습니다!")
            return
        
        # 진행바 시작
        self.progress.start(10)
        
        try:
            # 파일 읽기
            self.log("📖 파일 읽는 중... / Reading file...")
            
            with open(file_path, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            
            total_lines = len(lines)
            self.log(f"총 {total_lines}줄 읽음 / Read {total_lines} lines")
            
            # 구분자 자동 감지
            sep_type, separator = self.detect_separator(lines)
            
            if sep_type == 'tab':
                self.log(f"🔍 감지된 형식 / Detected format: 탭(\\t) 구분 / Tab separator")
            elif sep_type == 'arrow':
                self.log(f"🔍 감지된 형식 / Detected format: ==> 구분 / ==> separator")
            else:
                self.log(f"🔍 감지된 형식 / Detected format: = 구분 (변환 불필요) / = separator (no conversion needed)")
                self.log("✅ 이미 올바른 형식입니다 / Already in correct format")
                messagebox.showinfo("정보 / Info", "이미 올바른 형식입니다!\nAlready in correct format!")
                return
            
            # 변환 처리
            self.log(f"🔄 변환 중... ({separator} → =) / Converting... ({separator} → =)")
            converted_lines = []
            converted_count = 0
            skipped_count = 0
            
            for line in lines:
                line = line.strip()
                if not line:
                    continue
                
                # 구분자로 분리
                if separator in line:
                    parts = line.split(separator, 1)  # 최대 1번만 분리
                    if len(parts) == 2:
                        original = parts[0].strip()
                        translated = parts[1].strip()
                        converted_lines.append(f"{original}={translated}\n")
                        converted_count += 1
                    else:
                        skipped_count += 1
                else:
                    skipped_count += 1
            
            self.log(f"✅ {converted_count}개 항목 변환 완료 / {converted_count} entries converted")
            if skipped_count > 0:
                self.log(f"⚠️  {skipped_count}개 항목 건너뜀 (형식 불일치) / {skipped_count} entries skipped")
            
            # 출력 파일 경로 생성
            input_path = Path(file_path)
            output_path = input_path.parent / f"{input_path.stem}_converted{input_path.suffix}"
            
            # 파일 쓰기
            self.log(f"💾 저장 중... / Saving...")
            
            with open(output_path, 'w', encoding='utf-8') as f:
                f.writelines(converted_lines)
            
            self.log(f"✅ 저장 완료! / Save complete!")
            self.log(f"📁 출력 파일 / Output file: {output_path.name}")
            self.log(f"{'='*80}")
            
            # 완료 메시지
            messagebox.showinfo(
                "완료 / Complete",
                f"변환 완료!\nConverted!\n\n"
                f"입력 / Input: {total_lines}줄 / lines\n"
                f"출력 / Output: {converted_count}개 항목 / entries\n"
                f"건너뜀 / Skipped: {skipped_count}개 / entries\n\n"
                f"파일 / File: {output_path.name}"
            )
            
        except Exception as e:
            self.log(f"❌ 오류 발생 / Error: {str(e)}")
            messagebox.showerror("오류 / Error", f"변환 실패!\nConversion failed!\n\n{str(e)}")
            
        finally:
            # 진행바 중지
            self.progress.stop()


def main():
    """메인 함수"""
    if HAS_DND:
        root = tkdnd.Tk()
    else:
        root = tk.Tk()
    
    app = CacheConverterGUI(root)
    root.mainloop()


if __name__ == "__main__":
    main()
